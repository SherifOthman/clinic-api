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

        // One entry per day per schedule
        builder.HasIndex(w => new { w.DoctorBranchScheduleId, w.Day }).IsUnique();

        builder.ToTable(t =>
        {
            // DayOfWeek: 0=Sunday … 6=Saturday
            t.HasCheckConstraint("CK_WorkingDay_Day",       "[Day] BETWEEN 0 AND 6");
            t.HasCheckConstraint("CK_WorkingDay_TimeRange",  "[EndTime] > [StartTime]");
        });
    }
}
