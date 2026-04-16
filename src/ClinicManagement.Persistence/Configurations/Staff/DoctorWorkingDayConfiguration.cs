using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class DoctorWorkingDayConfiguration : IEntityTypeConfiguration<DoctorWorkingDay>
{
    public void Configure(EntityTypeBuilder<DoctorWorkingDay> builder)
    {
        // DoctorProfile nav — wire to Doctor.WorkingDays collection to avoid shadow FK.
        builder.HasOne(wd => wd.DoctorProfile)
            .WithMany(d => d.WorkingDays)
            .HasForeignKey(wd => wd.DoctorId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        // ClinicBranch nav — optional to avoid query filter conflict.
        // ClinicBranch has a tenant filter; making this optional suppresses the warning.
        builder.HasOne(wd => wd.ClinicBranch)
            .WithMany()
            .HasForeignKey(wd => wd.ClinicBranchId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(wd => wd.DoctorId);
    }
}
