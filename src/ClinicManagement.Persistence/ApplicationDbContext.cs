using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;
using System.Text.Json;

namespace ClinicManagement.Persistence;

public class ApplicationDbContext : IdentityDbContext<User, Role, Guid>
{
    private readonly ICurrentUserService? _currentUserService;

    // Fields that must never appear in audit diffs — hashes, tokens, internal EF fields
    private static readonly HashSet<string> ExcludedFromAudit = new(StringComparer.OrdinalIgnoreCase)
    {
        "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
        "NormalizedEmail", "NormalizedUserName",
        "RefreshToken", "TokenHash", "InvitationToken",
        "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy",  // stamped separately
    };

    // ── Location reference tables (GeoNames seed) ─────────────────────────────
    public DbSet<GeoCountry> GeoCountries => Set<GeoCountry>();
    public DbSet<GeoState> GeoStates => Set<GeoState>();
    public DbSet<GeoCity> GeoCities => Set<GeoCity>();

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService? currentUserService = null,
        IMemoryCache? cache = null)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now    = DateTimeOffset.UtcNow;
        var userId = _currentUserService?.UserId;

        // ── 1. Stamp CreatedAt/UpdatedAt/CreatedBy/UpdatedBy ─────────────────
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = userId;
            }
        }

        // ── 2. Capture audit entries BEFORE saving ────────────────────────────
        // OriginalValues are only available before base.SaveChangesAsync is called.
        var auditEntries = CaptureAuditEntries(now, userId);

        // ── 3. Save the actual changes ────────────────────────────────────────
        var result = await base.SaveChangesAsync(cancellationToken);

        // ── 4. Write audit logs in a second save ──────────────────────────────
        // Done after the main save so that:
        //   a) If the main save fails, no audit entry is written (correct — nothing happened)
        //   b) Newly created entity IDs are available (Guid is client-generated so this
        //      is actually fine either way, but the pattern is cleaner)
        if (auditEntries.Count > 0)
        {
            Set<AuditLog>().AddRange(auditEntries);
            await base.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    // ── Audit capture ─────────────────────────────────────────────────────────

    private List<AuditLog> CaptureAuditEntries(DateTimeOffset now, Guid? userId)
    {
        // Only process entities that opted in via IAuditableEntity
        var entries = ChangeTracker
            .Entries<IAuditableEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        if (entries.Count == 0) return [];

        var clinicId  = _currentUserService?.ClinicId;
        var fullName  = _currentUserService?.FullName;
        var username  = _currentUserService?.Username;
        var email     = _currentUserService?.Email;
        var role      = _currentUserService?.Roles.FirstOrDefault();
        var ipAddress = _currentUserService?.IpAddress;
        var userAgent = _currentUserService?.UserAgent;

        var auditLogs = new List<AuditLog>(entries.Count);

        foreach (var entry in entries)
        {
            var action = entry.State switch
            {
                EntityState.Added    => AuditAction.Create,
                EntityState.Modified => AuditAction.Update,
                EntityState.Deleted  => AuditAction.Delete,
                _                    => AuditAction.Update,
            };

            var changes = BuildChanges(entry, action);

            // Skip Update entries where nothing meaningful changed
            // (e.g. only excluded fields like UpdatedAt were touched)
            if (action == AuditAction.Update && changes is null) continue;

            // Resolve ClinicId: prefer current user's clinic, fall back to entity's own ClinicId
            var entityClinicId = clinicId
                ?? (entry.Entity is ITenantEntity tenant ? tenant.ClinicId : null);

            auditLogs.Add(new AuditLog
            {
                Timestamp  = now,
                ClinicId   = entityClinicId,
                UserId     = userId,
                FullName   = fullName,
                Username   = username,
                UserEmail  = email,
                UserRole   = role,
                IpAddress  = ipAddress,
                UserAgent  = userAgent,
                EntityType = entry.Entity.GetType().Name,
                EntityId   = entry.Property("Id").CurrentValue?.ToString() ?? "unknown",
                Action     = action,
                Changes    = changes,
            });
        }

        return auditLogs;
    }

    /// <summary>
    /// Builds a JSON diff string for the changed properties.
    /// Returns null if nothing meaningful changed (all modified props were excluded).
    /// For Create/Delete, returns a snapshot of all non-excluded properties.
    /// </summary>
    private static string? BuildChanges(EntityEntry<IAuditableEntity> entry, AuditAction action)
    {
        if (action == AuditAction.Create)
        {
            // Snapshot of all non-excluded properties at creation time
            var snapshot = entry.Properties
                .Where(p => !ExcludedFromAudit.Contains(p.Metadata.Name))
                .Where(p => p.CurrentValue is not null)
                .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);

            return snapshot.Count > 0 ? JsonSerializer.Serialize(snapshot) : null;
        }

        if (action == AuditAction.Delete)
        {
            // Snapshot of what was deleted
            var snapshot = entry.Properties
                .Where(p => !ExcludedFromAudit.Contains(p.Metadata.Name))
                .Where(p => p.OriginalValue is not null)
                .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);

            return snapshot.Count > 0 ? JsonSerializer.Serialize(snapshot) : null;
        }

        // Update — only include properties that actually changed
        var diff = entry.Properties
            .Where(p => p.IsModified && !ExcludedFromAudit.Contains(p.Metadata.Name))
            .Where(p => !Equals(p.OriginalValue, p.CurrentValue))
            .ToDictionary(
                p => p.Metadata.Name,
                p => new { Old = p.OriginalValue, New = p.CurrentValue });

        return diff.Count > 0 ? JsonSerializer.Serialize(diff) : null;
    }

    // ── Model configuration ───────────────────────────────────────────────────

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Rename Identity tables
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<Role>().ToTable("Roles");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");

        // Apply entity configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Apply tenant and soft-delete filters automatically to all matching entity types
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            if (typeof(ITenantEntity).IsAssignableFrom(clrType))
            {
                typeof(ApplicationDbContext)
                    .GetMethod(nameof(ApplyTenantFilter), BindingFlags.NonPublic | BindingFlags.Instance)!
                    .MakeGenericMethod(clrType)
                    .Invoke(this, [modelBuilder]);
            }

            if (typeof(ISoftDeletable).IsAssignableFrom(clrType))
            {
                typeof(ApplicationDbContext)
                    .GetMethod(nameof(ApplySoftDeletableFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(clrType)
                    .Invoke(null, [modelBuilder]);
            }
        }
    }

    private Guid CurrentClinicId => _currentUserService?.ClinicId ?? Guid.Empty;

    private void ApplyTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantEntity
    {
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(
                QueryFilterNames.Tenant,
                e => _currentUserService == null || e.ClinicId == CurrentClinicId);
    }

    private static void ApplySoftDeletableFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ISoftDeletable
    {
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(QueryFilterNames.SoftDelete, e => !e.IsDeleted);
    }
}
