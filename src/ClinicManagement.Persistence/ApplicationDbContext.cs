using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;

namespace ClinicManagement.Persistence;

public class ApplicationDbContext : IdentityDbContext<User, Role, Guid>
{
    private readonly ICurrentUserService? _currentUserService;
    private readonly AuditEntryBuilder _auditEntryBuilder;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService? currentUserService = null,
        IMemoryCache? cache = null)
        : base(options)
    {
        _currentUserService = currentUserService;
        _auditEntryBuilder = new AuditEntryBuilder();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var userId    = _currentUserService?.UserId;
        var ipAddress = _currentUserService?.IpAddress;
        var userRole  = _currentUserService?.Roles.FirstOrDefault();
        var fullName  = _currentUserService?.FullName;
        var username  = _currentUserService?.Username;
        var userEmail = _currentUserService?.Email;
        var userAgent = _currentUserService?.UserAgent;
        var now       = DateTimeOffset.UtcNow;

        var auditEntries = _auditEntryBuilder.Build(
            ChangeTracker.Entries<AuditableEntity>(),
            userId, fullName, username, userEmail, userRole, ipAddress, userAgent, now);

        // Stamp audit fields
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

        var result = await base.SaveChangesAsync(cancellationToken);

        // Persist audit logs after save (so EntityId is available for Added entries)
        if (auditEntries.Count > 0)
        {
            Set<AuditLog>().AddRange(auditEntries);
            await base.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

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

        // Apply entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Apply tenant filter to all ITenantEntity types automatically
        // Apply soft-delete filter to all AuditableEntity types automatically
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

            if (typeof(AuditableEntity).IsAssignableFrom(clrType))
            {
                typeof(ApplicationDbContext)
                    .GetMethod(nameof(ApplySoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)!
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

    private static void ApplySoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : AuditableEntity
    {
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(QueryFilterNames.SoftDelete, e => !e.IsDeleted);
    }
}
