using ClinicManagement.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.API.Infrastructure.Data.Configurations;

public class DoctorMeasurementAttributeConfiguration : IEntityTypeConfiguration<DoctorMeasurementAttribute>
{
    public void Configure(EntityTypeBuilder<DoctorMeasurementAttribute> builder)
    {
        builder.HasKey(dma => dma.Id);

        builder.HasOne(dma => dma.DoctorProfile)
            .WithMany(d => d.MeasurementAttributes)
            .HasForeignKey(dma => dma.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(dma => dma.MeasurementAttribute)
            .WithMany(ma => ma.DoctorMeasurements)
            .HasForeignKey(dma => dma.MeasurementAttributeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(dma => new { dma.DoctorId, dma.MeasurementAttributeId })
            .IsUnique();
    }
}
