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

        // Value objects
        builder.Property(md => md.UnitPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(md => md.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(md => md.Notes)
            .HasMaxLength(500);
    }
}
