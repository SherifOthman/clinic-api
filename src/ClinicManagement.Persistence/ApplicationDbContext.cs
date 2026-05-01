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
        var userId = _currentUserService?.UserId;
        var now    = DateTimeOffset.UtcNow;

        // Stamp CreatedAt/UpdatedAt/CreatedBy/UpdatedBy on all auditable entities
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

        return await base.SaveChangesAsync(cancellationToken);
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
