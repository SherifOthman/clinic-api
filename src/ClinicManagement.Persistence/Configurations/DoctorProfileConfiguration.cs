using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class DoctorProfileConfiguration : IEntityTypeConfiguration<DoctorProfile>
{
    public void Configure(EntityTypeBuilder<DoctorProfile> builder)
    {
        // Staff nav — back-reference from DoctorProfile to Staff
        // Relationship owned by StaffConfiguration (HasOne(s => s.DoctorProfile).WithOne())
        // We configure the inverse here so EF knows about it
        builder.HasOne(dp => dp.Staff)
            .WithOne(s => s.DoctorProfile)
            .HasForeignKey<DoctorProfile>(dp => dp.StaffId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(dp => dp.StaffId).IsUnique();

        builder.HasOne(dp => dp.Specialization)
            .WithMany()
            .HasForeignKey(dp => dp.SpecializationId)
            .OnDelete(DeleteBehavior.Restrict);

        // DoctorWorkingDay relationship configured in DoctorWorkingDayConfiguration
        // using HasOne(wd => wd.DoctorProfile).WithMany().IsRequired(false)
    }
}
