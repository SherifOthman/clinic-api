using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class ClinicMemberConfiguration : IEntityTypeConfiguration<ClinicMember>
{
    public void Configure(EntityTypeBuilder<ClinicMember> builder)
    {
        builder.Property(m => m.Role).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.ToTable(t => t.HasCheckConstraint("CK_ClinicMember_Role",
            "[Role] IN ('Owner', 'Doctor', 'Receptionist', 'Nurse')"));

        // A user can be a member at multiple clinics, but only once per clinic
        builder.HasIndex(m => new { m.UserId, m.ClinicId }).IsUnique()
            .HasFilter("[UserId] IS NOT NULL");
        builder.HasIndex(m => new { m.ClinicId, m.IsActive });

        // Only one Owner per clinic — enforced at DB level
        builder.HasIndex(m => new { m.ClinicId, m.Role })
            .IsUnique()
            .HasFilter("[Role] = 'Owner'");

        builder.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Clinic)
            .WithMany(c => c.Members)
            .HasForeignKey(m => m.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
