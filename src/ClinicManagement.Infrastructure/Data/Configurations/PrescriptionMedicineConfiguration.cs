using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class PrescriptionMedicineConfiguration : IEntityTypeConfiguration<PrescriptionMedicine>
{
    public void Configure(EntityTypeBuilder<PrescriptionMedicine> builder)
    {
        builder.ToTable("PrescriptionMedicines");
        builder.Property(e => e.DosageInstructions).HasMaxLength(100);
        builder.Property(e => e.Duration).HasMaxLength(50);
        builder.Property(e => e.Notes).HasMaxLength(200);
        builder.HasOne(d => d.Visit)
            .WithMany(p => p.PrescriptionMedicines)
            .HasForeignKey(d => d.VisitId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(d => d.Medicine)
            .WithMany(p => p.PrescriptionMedicines)
            .HasForeignKey(d => d.MedicineId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

