using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class RoleDefaultPermissionConfiguration
    : IEntityTypeConfiguration<RoleDefaultPermission>
{
    public void Configure(EntityTypeBuilder<RoleDefaultPermission> builder)
    {
        builder.ToTable("RoleDefaultPermissions");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Role)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.Permission)
            .HasConversion<string>()
            .HasMaxLength(100)
            .IsRequired();

        // Unique constraint — one row per role+permission pair
        builder.HasIndex(r => new { r.Role, r.Permission })
            .IsUnique();
    }
}
