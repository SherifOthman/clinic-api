using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public DbSet<Review> Reviews => Set<Review>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Rename Identity tables
        builder.Entity<IdentityRole<int>>().ToTable("Roles");
        builder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");

        builder.Entity<IdentityRole<int>>().HasData(GetRolesSeed());

        // Reseed identity to start after seed data
        builder.Entity<IdentityRole<int>>().Property(r => r.Id).UseIdentityColumn(7, 1);

        // Apply all configurations
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

      
    }

    private List<IdentityRole<int>> GetRolesSeed()
    {
        return new List<IdentityRole<int>>()
    {
        new IdentityRole<int>(UserRole.ClinicOwner.ToString()) 
        { 
            Id = 1, 
            NormalizedName = UserRole.ClinicOwner.ToString().ToUpper(),
            ConcurrencyStamp = "1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d"
        },
        new IdentityRole<int>(UserRole.Doctor.ToString()) 
        { 
            Id = 2, 
            NormalizedName = UserRole.Doctor.ToString().ToUpper(),
            ConcurrencyStamp = "2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e"
        },
        new IdentityRole<int>(UserRole.Patient.ToString()) 
        { 
            Id = 3, 
            NormalizedName = UserRole.Patient.ToString().ToUpper(),
            ConcurrencyStamp = "3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f"
        },
        new IdentityRole<int>(UserRole.Receptionist.ToString()) 
        { 
            Id = 4, 
            NormalizedName = UserRole.Receptionist.ToString().ToUpper(),
            ConcurrencyStamp = "4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"
        },
        new IdentityRole<int>(UserRole.Nurse.ToString()) 
        { 
            Id = 5, 
            NormalizedName = UserRole.Nurse.ToString().ToUpper(),
            ConcurrencyStamp = "5e6f7a8b-9c0d-1e2f-3a4b-5c6d7e8f9a0b"
        },
        new IdentityRole<int>(UserRole.SystemAdmin.ToString()) 
        { 
            Id = 6, 
            NormalizedName = UserRole.SystemAdmin.ToString().ToUpper(),
            ConcurrencyStamp = "6f7a8b9c-0d1e-2f3a-4b5c-6d7e8f9a0b1c"
        },
    };
    }
}
