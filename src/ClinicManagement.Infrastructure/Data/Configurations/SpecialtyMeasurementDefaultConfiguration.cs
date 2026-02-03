using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class SpecialtyMeasurementDefaultConfiguration : IEntityTypeConfiguration<SpecialtyMeasurementDefault>
{
    public void Configure(EntityTypeBuilder<SpecialtyMeasurementDefault> builder)
    {
        builder.ToTable("SpecialtyMeasurementDefaults");
        
        // Primary key
        builder.HasKey(smd => smd.Id);
        
        // Properties
        builder.Property(smd => smd.SpecialtyId).IsRequired();
        builder.Property(smd => smd.MeasurementDefinitionId).IsRequired();
        builder.Property(smd => smd.IsRequired).IsRequired();
        builder.Property(smd => smd.DisplayOrder).IsRequired();
        
        // Relationships
        builder.HasOne(smd => smd.Specialty)
            .WithMany(s => s.MeasurementDefaults)
            .HasForeignKey(smd => smd.SpecialtyId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(smd => smd.MeasurementDefinition)
            .WithMany(md => md.SpecialtyDefaults)
            .HasForeignKey(smd => smd.MeasurementDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes
        builder.HasIndex(smd => smd.SpecialtyId);
        builder.HasIndex(smd => smd.MeasurementDefinitionId);
        builder.HasIndex(smd => new { smd.SpecialtyId, smd.DisplayOrder });
        
        // Unique constraint
        builder.HasIndex(smd => new { smd.SpecialtyId, smd.MeasurementDefinitionId })
            .IsUnique();
    }
}