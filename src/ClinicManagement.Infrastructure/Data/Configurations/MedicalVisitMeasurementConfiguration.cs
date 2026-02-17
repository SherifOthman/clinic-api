using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class MedicalVisitMeasurementConfiguration : IEntityTypeConfiguration<MedicalVisitMeasurement>
{
    public void Configure(EntityTypeBuilder<MedicalVisitMeasurement> builder)
    {
        builder.HasKey(mvm => mvm.Id);

        builder.Property(mvm => mvm.StringValue)
            .HasMaxLength(255);

        builder.Property(mvm => mvm.DecimalValue)
            .HasPrecision(18, 4);

        builder.Property(mvm => mvm.Notes)
            .HasMaxLength(500);

        builder.HasOne(mvm => mvm.MedicalVisit)
            .WithMany(mv => mv.Measurements)
            .HasForeignKey(mvm => mvm.MedicalVisitId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(mvm => mvm.MeasurementAttribute)
            .WithMany(ma => ma.VisitMeasurements)
            .HasForeignKey(mvm => mvm.MeasurementAttributeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(mvm => new { mvm.MedicalVisitId, mvm.MeasurementAttributeId })
            .IsUnique();
    }
}
