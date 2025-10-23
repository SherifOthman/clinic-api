using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class MedicineConfiguration : IEntityTypeConfiguration<Medicine>
{
    public void Configure(EntityTypeBuilder<Medicine> builder)
    {
        builder.ToTable("Medicines");
        builder.Property(e => e.Name).HasMaxLength(100);
        builder.Property(e => e.GenericName).HasMaxLength(100);
        builder.Property(e => e.Dosage).HasMaxLength(50);
        builder.Property(e => e.Form).HasMaxLength(50);
        builder.Property(e => e.Description).HasMaxLength(300);
    }
}

