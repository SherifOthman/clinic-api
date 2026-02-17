using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class LabTestOrderConfiguration : IEntityTypeConfiguration<LabTestOrder>
{
    public void Configure(EntityTypeBuilder<LabTestOrder> builder)
    {
        builder.ToTable("LabTestOrder");

        builder.HasKey(lto => lto.Id);

        // Configure relationships with NO ACTION to avoid cascade cycles
        builder.HasOne(lto => lto.ClinicBranch)
            .WithMany()
            .HasForeignKey(lto => lto.ClinicBranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(lto => lto.Patient)
            .WithMany()
            .HasForeignKey(lto => lto.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(lto => lto.LabTest)
            .WithMany()
            .HasForeignKey(lto => lto.LabTestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(lto => lto.MedicalVisit)
            .WithMany()
            .HasForeignKey(lto => lto.MedicalVisitId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(lto => lto.OrderedByDoctor)
            .WithMany()
            .HasForeignKey(lto => lto.OrderedByDoctorId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Properties
        builder.Property(lto => lto.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(lto => lto.ResultFilePath)
            .HasMaxLength(255);

        builder.Property(lto => lto.ResultNotes)
            .HasMaxLength(2000);

        builder.Property(lto => lto.DoctorNotes)
            .HasMaxLength(1000);

        builder.Property(lto => lto.Notes)
            .HasMaxLength(500);
    }
}
