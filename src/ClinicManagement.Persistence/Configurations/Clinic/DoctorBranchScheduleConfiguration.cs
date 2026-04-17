using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class DoctorBranchScheduleConfiguration : IEntityTypeConfiguration<DoctorBranchSchedule>
{
    public void Configure(EntityTypeBuilder<DoctorBranchSchedule> builder)
    {
        // A doctor can only have one schedule per branch
        builder.HasIndex(s => new { s.DoctorInfoId, s.BranchId }).IsUnique();

        builder.HasOne(s => s.DoctorInfo)
            .WithMany(d => d.BranchSchedules)
            .HasForeignKey(s => s.DoctorInfoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Branch)
            .WithMany(b => b.DoctorSchedules)
            .HasForeignKey(s => s.BranchId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
