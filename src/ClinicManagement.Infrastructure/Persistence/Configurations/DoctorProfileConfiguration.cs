using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Persistence.Configurations;

public class DoctorProfileConfiguration : IEntityTypeConfiguration<DoctorProfile>
{
    public void Configure(EntityTypeBuilder<DoctorProfile> builder)
    {
        builder.Property(dp => dp.LicenseNumber).HasMaxLength(100);
        builder.Property(dp => dp.Bio).HasMaxLength(1000);
        
        builder.HasOne(dp => dp.Staff)
            .WithOne(s => s.DoctorProfile)
            .HasForeignKey<DoctorProfile>(dp => dp.StaffId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(dp => dp.Specialization)
            .WithMany()
            .HasForeignKey(dp => dp.SpecializationId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(dp => dp.DoctorSpecializations)
            .WithOne(ds => ds.DoctorProfile)
            .HasForeignKey(ds => ds.DoctorProfileId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(dp => dp.WorkingDays)
            .WithOne()
            .HasForeignKey(wd => wd.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
