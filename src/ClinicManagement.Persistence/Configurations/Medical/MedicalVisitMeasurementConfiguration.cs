using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class MedicalVisitMeasurementConfiguration : IEntityTypeConfiguration<MedicalVisitMeasurement>
{
    public void Configure(EntityTypeBuilder<MedicalVisitMeasurement> builder)
    {
        builder.Property(mvm => mvm.DecimalValue)
            .HasPrecision(18, 4);
    }
}
