using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Persistence.Configurations;

public class DoctorSpecializationConfiguration : IEntityTypeConfiguration<DoctorSpecialization>
{
    public void Configure(EntityTypeBuilder<DoctorSpecialization> builder)
    {
        builder.Property(ds => ds.CertificationNumber).HasMaxLength(100);
        
        builder.HasOne(ds => ds.DoctorProfile)
            .WithMany(dp => dp.DoctorSpecializations)
            .HasForeignKey(ds => ds.DoctorProfileId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(ds => ds.Specialization)
            .WithMany()
            .HasForeignKey(ds => ds.SpecializationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
