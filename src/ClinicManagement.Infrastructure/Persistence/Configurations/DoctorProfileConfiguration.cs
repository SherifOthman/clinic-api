using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Persistence.Configurations;

public class DoctorProfileConfiguration : IEntityTypeConfiguration<DoctorProfile>
{
    public void Configure(EntityTypeBuilder<DoctorProfile> builder)
    {
        // Staff nav removed from DoctorProfile — relationship owned by StaffConfiguration
        builder.HasOne(dp => dp.Specialization)
            .WithMany()
            .HasForeignKey(dp => dp.SpecializationId)
            .OnDelete(DeleteBehavior.Restrict);

        // WorkingDays nav removed from DoctorProfile — configure FK only
        builder.HasMany<DoctorWorkingDay>()
            .WithOne()
            .HasForeignKey(wd => wd.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
