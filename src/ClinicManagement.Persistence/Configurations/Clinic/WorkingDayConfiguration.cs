using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class WorkingDayConfiguration : IEntityTypeConfiguration<WorkingDay>
{
    public void Configure(EntityTypeBuilder<WorkingDay> builder)
    {
        builder.HasOne(w => w.Schedule)
            .WithMany(s => s.WorkingDays)
            .HasForeignKey(w => w.DoctorBranchScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => w.DoctorBranchScheduleId);
    }
}
