using ClinicManagement.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.API.Infrastructure.Data.Configurations;

public class RadiologyOrderConfiguration : IEntityTypeConfiguration<RadiologyOrder>
{
    public void Configure(EntityTypeBuilder<RadiologyOrder> builder)
    {
        builder.ToTable("RadiologyOrder");

        builder.HasKey(ro => ro.Id);

        // Configure relationships with NO ACTION to avoid cascade cycles
        builder.HasOne(ro => ro.ClinicBranch)
            .WithMany()
            .HasForeignKey(ro => ro.ClinicBranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ro => ro.Patient)
            .WithMany()
            .HasForeignKey(ro => ro.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ro => ro.RadiologyTest)
            .WithMany()
            .HasForeignKey(ro => ro.RadiologyTestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ro => ro.MedicalVisit)
            .WithMany()
            .HasForeignKey(ro => ro.MedicalVisitId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(ro => ro.OrderedByDoctor)
            .WithMany()
            .HasForeignKey(ro => ro.OrderedByDoctorId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Properties
        builder.Property(ro => ro.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(ro => ro.ImageFilePath)
            .HasMaxLength(255);

        builder.Property(ro => ro.ReportFilePath)
            .HasMaxLength(255);

        builder.Property(ro => ro.Findings)
            .HasMaxLength(2000);

        builder.Property(ro => ro.DoctorNotes)
            .HasMaxLength(1000);

        builder.Property(ro => ro.Notes)
            .HasMaxLength(500);
    }
}
