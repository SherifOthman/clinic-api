using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class DoctorInfoConfiguration : IEntityTypeConfiguration<DoctorInfo>
{
    public void Configure(EntityTypeBuilder<DoctorInfo> builder)
    {
        builder.Property(d => d.LicenseNumber).HasMaxLength(100);

        builder.HasOne(d => d.ClinicMember)
            .WithOne(m => m.DoctorInfo)
            .HasForeignKey<DoctorInfo>(d => d.ClinicMemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Specialization)
            .WithMany()
            .HasForeignKey(d => d.SpecializationId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => d.ClinicMemberId).IsUnique();
    }
}
