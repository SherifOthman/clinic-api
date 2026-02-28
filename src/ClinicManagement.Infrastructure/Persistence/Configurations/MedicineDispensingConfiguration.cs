using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Persistence.Configurations;

public class MedicineDispensingConfiguration : IEntityTypeConfiguration<MedicineDispensing>
{
    public void Configure(EntityTypeBuilder<MedicineDispensing> builder)
    {
        builder.Property(md => md.UnitPrice)
            .HasPrecision(18, 2);
    }
}
