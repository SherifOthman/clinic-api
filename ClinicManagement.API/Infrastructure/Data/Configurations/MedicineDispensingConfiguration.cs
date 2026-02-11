using ClinicManagement.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.API.Infrastructure.Data.Configurations;

public class MedicineDispensingConfiguration : IEntityTypeConfiguration<MedicineDispensing>
{
    public void Configure(EntityTypeBuilder<MedicineDispensing> builder)
    {
        builder.ToTable("MedicineDispensing");

        builder.HasKey(md => md.Id);

        // Configure relationships with NO ACTION to avoid cascade cycles
        builder.HasOne(md => md.ClinicBranch)
            .WithMany()
            .HasForeignKey(md => md.ClinicBranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(md => md.Patient)
            .WithMany()
            .HasForeignKey(md => md.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(md => md.Medicine)
            .WithMany()
            .HasForeignKey(md => md.MedicineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(md => md.Visit)
            .WithMany()
            .HasForeignKey(md => md.VisitId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(md => md.Prescription)
            .WithMany()
            .HasForeignKey(md => md.PrescriptionId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(md => md.DispensedBy)
            .WithMany()
            .HasForeignKey(md => md.DispensedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Value objects
        builder.Property(md => md.UnitPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(md => md.TotalPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(md => md.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(md => md.Unit)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(md => md.Notes)
            .HasMaxLength(1000);
    }
}
