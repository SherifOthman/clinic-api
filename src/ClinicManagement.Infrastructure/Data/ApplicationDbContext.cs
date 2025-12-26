using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ClinicManagement.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>, IApplicationDbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    // Core entities
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<ClinicBranch> ClinicBranches => Set<ClinicBranch>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Receptionist> Receptionists => Set<Receptionist>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options, 
        IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Set audit fields
            var entries = ChangeTracker.Entries<AuditableEntity>();
            
            // Get current user ID from HTTP context
            int? currentUserId = null;
            var userIdClaim = _httpContextAccessor?.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                currentUserId = userId;
            }
            
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = currentUserId;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = currentUserId;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Log the concurrency exception for debugging
            // In a real application, you'd use proper logging here
            System.Diagnostics.Debug.WriteLine($"Concurrency exception in SaveChangesAsync: {ex.Message}");
            
            // Re-throw the exception to let the caller handle it
            throw;
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Rename Identity tables
        builder.Entity<User>().ToTable("Users");
        builder.Entity<IdentityRole<int>>().ToTable("Roles");
        builder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");

        builder.Entity<IdentityRole<int>>().HasData(GetRolesSeed());

        // Apply all configurations
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filters for soft deletes
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var parameter = Expression.Parameter(entityType.ClrType, "e");
            Expression? filter = null;

            // Soft delete filter
            if (typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                var isDeletedProperty = Expression.Property(parameter, nameof(AuditableEntity.IsDeleted));
                filter = Expression.Equal(isDeletedProperty, Expression.Constant(false));
            }

            if (filter != null)
            {
                var lambda = Expression.Lambda(filter, parameter);
                entityType.SetQueryFilter(lambda);
            }
        }
    }

    private List<IdentityRole<int>> GetRolesSeed()
    {
        return new List<IdentityRole<int>>()
        {
            new IdentityRole<int>("ClinicOwner") 
            { 
                Id = 1, 
                NormalizedName = "CLINICOWNER",
                ConcurrencyStamp = "1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d"
            },
            new IdentityRole<int>("Doctor") 
            { 
                Id = 2, 
                NormalizedName = "DOCTOR",
                ConcurrencyStamp = "2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e"
            },
            new IdentityRole<int>("Receptionist") 
            { 
                Id = 3, 
                NormalizedName = "RECEPTIONIST",
                ConcurrencyStamp = "3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f"
            }
        };
    }
}
