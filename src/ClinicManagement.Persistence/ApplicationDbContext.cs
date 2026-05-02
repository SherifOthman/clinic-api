using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Persistence.Audit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ClinicManagement.Persistence;

public class ApplicationDbContext : IdentityDbContext<User, Role, Guid>
{
    private readonly ICurrentUserService _currentUserService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService)
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
        var userId = _currentUserService.UserId;

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

    // EF Core's HasQueryFilter requires a generic type parameter at compile time.
    // These two methods are the bridge: the first dispatches via reflection using
    // the runtime Type, the second does the actual work with the compile-time T.

    private void ApplyTenantFilter(ModelBuilder modelBuilder, Type entityType)
        => typeof(ApplicationDbContext)
            .GetMethod(nameof(ApplyTenantFilter), BindingFlags.NonPublic | BindingFlags.Instance, [typeof(ModelBuilder)])!
            .MakeGenericMethod(entityType)
            .Invoke(this, [modelBuilder]);

    private void ApplyTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantEntity
        => modelBuilder.Entity<TEntity>()
            .HasQueryFilter(
                QueryFilterNames.Tenant,
                e => _currentUserService.ClinicId == null
                     || e.ClinicId == _currentUserService.ClinicId.Value);

    private static void ApplySoftDeleteFilter(ModelBuilder modelBuilder, Type entityType)
        => typeof(ApplicationDbContext)
            .GetMethod(nameof(ApplySoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static, [typeof(ModelBuilder)])!
            .MakeGenericMethod(entityType)
            .Invoke(null, [modelBuilder]);

    private static void ApplySoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ISoftDeletable
        => modelBuilder.Entity<TEntity>()
            .HasQueryFilter(QueryFilterNames.SoftDelete, e => !e.IsDeleted);
}
