using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class DoctorMeasurementConfiguration : IEntityTypeConfiguration<DoctorMeasurement>
{
    public void Configure(EntityTypeBuilder<DoctorMeasurement> builder)
    {
        builder.ToTable("DoctorMeasurements");
        
        // Primary key
        builder.HasKey(dm => dm.Id);
        
        // Properties
        builder.Property(dm => dm.DoctorId).IsRequired();
        builder.Property(dm => dm.MeasurementDefinitionId).IsRequired();
        builder.Property(dm => dm.IsEnabled).IsRequired();
        builder.Property(dm => dm.IsRequired).IsRequired();
        builder.Property(dm => dm.DisplayOrder).IsRequired();
        
        // Relationships
        builder.HasOne(dm => dm.Doctor)
            .WithMany(u => u.DoctorMeasurements)
            .HasForeignKey(dm => dm.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(dm => dm.MeasurementDefinition)
            .WithMany(md => md.DoctorMeasurements)
            .HasForeignKey(dm => dm.MeasurementDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes
        builder.HasIndex(dm => dm.DoctorId);
        builder.HasIndex(dm => dm.MeasurementDefinitionId);
        builder.HasIndex(dm => new { dm.DoctorId, dm.IsEnabled });
        builder.HasIndex(dm => new { dm.DoctorId, dm.DisplayOrder });
        
        // Unique constraint
        builder.HasIndex(dm => new { dm.DoctorId, dm.MeasurementDefinitionId })
            .IsUnique();
    }
}