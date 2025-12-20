using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<int>>
{
    public void Configure(EntityTypeBuilder<IdentityUserRole<int>> builder)
    {
        // Seed SystemAdmin user role assignment
        builder.HasData(
            new IdentityUserRole<int>
            {
                UserId = 1, // SystemAdmin user
                RoleId = 6  // SystemAdmin role
            }
        );
    }
}
