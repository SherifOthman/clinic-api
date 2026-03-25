using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Persistence.Configurations;

public class MedicalSupplyConfiguration : IEntityTypeConfiguration<MedicalSupply>
{
    public void Configure(EntityTypeBuilder<MedicalSupply> builder)
    {
        builder.Property(ms => ms.UnitPrice)
            .HasPrecision(18, 2);
    }
}
