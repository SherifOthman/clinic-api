using ClinicManagement.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.API.Infrastructure.Data.Configurations;

public class DoctorWorkingDayConfiguration : IEntityTypeConfiguration<DoctorWorkingDay>
{
    public void Configure(EntityTypeBuilder<DoctorWorkingDay> builder)
    {
        builder.ToTable("DoctorWorkingDays");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Day)
            .IsRequired();

        builder.Property(d => d.StartTime)
            .IsRequired();

        builder.Property(d => d.EndTime)
            .IsRequired();

        builder.Property(d => d.IsAvailable)
            .IsRequired()
            .HasDefaultValue(true);

        // Relationships
        builder.HasOne(d => d.DoctorProfile)
            .WithMany(doc => doc.WorkingDays)
            .HasForeignKey(d => d.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.ClinicBranch)
            .WithMany(cb => cb.DoctorWorkingDays)
            .HasForeignKey(d => d.ClinicBranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(d => new { d.DoctorId, d.ClinicBranchId, d.Day })
            .IsUnique()
            .HasDatabaseName("IX_DoctorWorkingDays_Doctor_Branch_Day");
    }
}
