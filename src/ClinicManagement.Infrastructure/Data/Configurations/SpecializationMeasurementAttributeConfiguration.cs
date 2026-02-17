using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class SpecializationMeasurementAttributeConfiguration : IEntityTypeConfiguration<SpecializationMeasurementAttribute>
{
    public void Configure(EntityTypeBuilder<SpecializationMeasurementAttribute> builder)
    {
        builder.HasKey(sma => sma.Id);

        builder.HasOne(sma => sma.Specialization)
            .WithMany(s => s.DefaultMeasurementAttributes)
            .HasForeignKey(sma => sma.SpecializationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sma => sma.MeasurementAttribute)
            .WithMany(ma => ma.SpecializationDefaults)
            .HasForeignKey(sma => sma.MeasurementAttributeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(sma => new { sma.SpecializationId, sma.MeasurementAttributeId })
            .IsUnique();
    }
}
