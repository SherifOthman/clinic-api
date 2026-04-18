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

        // A person can be a member at multiple clinics, but only once per clinic
        builder.HasIndex(m => new { m.PersonId, m.ClinicId }).IsUnique();
        builder.HasIndex(m => new { m.ClinicId, m.IsDeleted, m.IsActive });

        builder.HasOne(m => m.Person)
            .WithMany(p => p.ClinicMemberships)
            .HasForeignKey(m => m.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Clinic>()
            .WithMany(c => c.Members)
            .HasForeignKey(m => m.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
