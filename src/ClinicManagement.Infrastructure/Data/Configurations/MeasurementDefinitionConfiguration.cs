using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class MeasurementDefinitionConfiguration : IEntityTypeConfiguration<MeasurementDefinition>
{
    public void Configure(EntityTypeBuilder<MeasurementDefinition> builder)
    {
        builder.ToTable("MeasurementDefinitions");
        
        // Primary key
        builder.HasKey(md => md.Id);
        
        // Properties
        builder.Property(md => md.Code)
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(md => md.Name)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(md => md.Description)
            .HasMaxLength(1000)
            .IsRequired(false);
        builder.Property(md => md.DataType).IsRequired();
        builder.Property(md => md.Unit)
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(md => md.NormalRange)
            .HasMaxLength(200)
            .IsRequired(false);
        builder.Property(md => md.IsActive).IsRequired();
        builder.Property(md => md.HasMultipleValues).IsRequired();
        builder.Property(md => md.ValueLabels)
            .HasMaxLength(500)
            .IsRequired(false);
        
        // Collections
        builder.HasMany(md => md.SpecialtyDefaults)
            .WithOne(smd => smd.MeasurementDefinition)
            .HasForeignKey(smd => smd.MeasurementDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(md => md.DoctorMeasurements)
            .WithOne(dm => dm.MeasurementDefinition)
            .HasForeignKey(dm => dm.MeasurementDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(md => md.VisitMeasurements)
            .WithOne(vm => vm.MeasurementDefinition)
            .HasForeignKey(vm => vm.MeasurementDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes
        builder.HasIndex(md => md.Code).IsUnique();
        builder.HasIndex(md => md.Name);
        builder.HasIndex(md => md.DataType);
        builder.HasIndex(md => md.IsActive);
    }
}