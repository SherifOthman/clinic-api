using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class MedicalVisitMeasurementConfiguration : IEntityTypeConfiguration<MedicalVisitMeasurement>
{
    public void Configure(EntityTypeBuilder<MedicalVisitMeasurement> builder)
    {
        // JSON column — replaces the EAV pattern (StringValue, IntValue, DecimalValue, etc.)
        // Values are stored as a JSON object keyed by attribute name.
        // SQL Server 2022+ supports JSON path indexes on nvarchar(max) columns.
        builder.Property(m => m.ValuesJson)
            .HasColumnType("nvarchar(max)")
            .IsRequired()
            .HasDefaultValue("{}");

        builder.Property(m => m.Notes)
            .HasMaxLength(1000);

        builder.HasIndex(m => new { m.MedicalVisitId, m.MeasurementAttributeId })
            .IsUnique();
    }
}
