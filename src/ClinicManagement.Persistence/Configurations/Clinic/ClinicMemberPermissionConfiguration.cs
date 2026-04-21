using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class ClinicMemberPermissionConfiguration : IEntityTypeConfiguration<ClinicMemberPermission>
{
    public void Configure(EntityTypeBuilder<ClinicMemberPermission> builder)
    {
        builder.Property(p => p.Permission).HasConversion<string>().HasMaxLength(50).IsRequired();

        // One row per permission per member
        builder.HasIndex(p => new { p.ClinicMemberId, p.Permission }).IsUnique();

        builder.HasOne(p => p.ClinicMember)
            .WithMany(m => m.Permissions)
            .HasForeignKey(p => p.ClinicMemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
