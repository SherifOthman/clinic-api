using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Persistence.Audit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;

namespace ClinicManagement.Persistence;

public class ApplicationDbContext : IdentityDbContext<User, Role, Guid>
{
    private readonly ICurrentUserService? _currentUserService;

    // ── Location reference tables (GeoNames seed) ─────────────────────────────
    public DbSet<GeoCountry> GeoCountries => Set<GeoCountry>();
    public DbSet<GeoState>   GeoStates    => Set<GeoState>();
    public DbSet<GeoCity>    GeoCities    => Set<GeoCity>();

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
        var now = DateTimeOffset.UtcNow;

        StampAuditFields(now);

        // Capture diffs BEFORE saving — OriginalValues are only available here
        var auditLogs = AuditChangeTracker.Capture(ChangeTracker, _currentUserService, now);

        var result = await base.SaveChangesAsync(cancellationToken);

        // Write audit logs AFTER the main save succeeds
        // A failed main save produces no audit entry — correct, nothing happened
        if (auditLogs.Count > 0)
        {
            Set<AuditLog>().AddRange(auditLogs);
            await base.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    // ── Model configuration ───────────────────────────────────────────────────

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Rename Identity tables to cleaner names
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<Role>().ToTable("Roles");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");

        // Apply all IEntityTypeConfiguration<T> implementations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Apply tenant and soft-delete query filters to all matching entity types
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            if (typeof(ITenantEntity).IsAssignableFrom(clrType))
                ApplyTenantFilter(modelBuilder, clrType);

            if (typeof(ISoftDeletable).IsAssignableFrom(clrType))
                ApplySoftDeleteFilter(modelBuilder, clrType);
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void StampAuditFields(DateTimeOffset now)
    {
        var userId = _currentUserService?.UserId;

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
    }

    private void ApplyTenantFilter(ModelBuilder modelBuilder, Type clrType)
        => typeof(ApplicationDbContext)
            .GetMethod(nameof(ApplyTenantFilterGeneric), BindingFlags.NonPublic | BindingFlags.Instance)!
            .MakeGenericMethod(clrType)
            .Invoke(this, [modelBuilder]);

    private void ApplyTenantFilterGeneric<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantEntity
        => modelBuilder.Entity<TEntity>()
            .HasQueryFilter(
                QueryFilterNames.Tenant,
                e => _currentUserService == null || e.ClinicId == (_currentUserService.ClinicId ?? Guid.Empty));

    private static void ApplySoftDeleteFilter(ModelBuilder modelBuilder, Type clrType)
        => typeof(ApplicationDbContext)
            .GetMethod(nameof(ApplySoftDeleteFilterGeneric), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(clrType)
            .Invoke(null, [modelBuilder]);

    private static void ApplySoftDeleteFilterGeneric<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ISoftDeletable
        => modelBuilder.Entity<TEntity>()
            .HasQueryFilter(QueryFilterNames.SoftDelete, e => !e.IsDeleted);
}
