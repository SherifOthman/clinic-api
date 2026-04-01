using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Persistence.Configurations;

public class DoctorProfileConfiguration : IEntityTypeConfiguration<DoctorProfile>
{
    public void Configure(EntityTypeBuilder<DoctorProfile> builder)
    {
        
        builder.HasOne(dp => dp.Staff)
            .WithOne(s => s.DoctorProfile)
            .HasForeignKey<DoctorProfile>(dp => dp.StaffId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(dp => dp.Specialization)
            .WithMany()
            .HasForeignKey(dp => dp.SpecializationId)
            .OnDelete(DeleteBehavior.Restrict);
        
        
        builder.HasMany(dp => dp.WorkingDays)
            .WithOne()
            .HasForeignKey(wd => wd.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
