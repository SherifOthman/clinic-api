using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class VisitMeasurementConfiguration : IEntityTypeConfiguration<VisitMeasurement>
{
    public void Configure(EntityTypeBuilder<VisitMeasurement> builder)
    {
        builder.ToTable("VisitMeasurements");
        
        // Primary key
        builder.HasKey(vm => vm.Id);
        
        // Properties
        builder.Property(vm => vm.VisitId).IsRequired();
        builder.Property(vm => vm.MeasurementDefinitionId).IsRequired();
        builder.Property(vm => vm.MeasurementDate).IsRequired();
        builder.Property(vm => vm.MeasuredByUserId).IsRequired();
        
        // Value properties - only one should be populated
        builder.Property(vm => vm.ValueInt).IsRequired(false);
        builder.Property(vm => vm.ValueDecimal)
            .HasPrecision(18, 4)
            .IsRequired(false);
        builder.Property(vm => vm.ValueText)
            .HasMaxLength(500)
            .IsRequired(false);
        builder.Property(vm => vm.ValueBool).IsRequired(false);
        builder.Property(vm => vm.ValueDate).IsRequired(false);
        builder.Property(vm => vm.StructuredValue)
            .HasMaxLength(2000)
            .IsRequired(false);
        builder.Property(vm => vm.Notes)
            .HasMaxLength(1000)
            .IsRequired(false);
        
        // Relationships
        builder.HasOne(vm => vm.Visit)
            .WithMany(v => v.Measurements)
            .HasForeignKey(vm => vm.VisitId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(vm => vm.MeasurementDefinition)
            .WithMany(md => md.VisitMeasurements)
            .HasForeignKey(vm => vm.MeasurementDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(vm => vm.MeasuredByUser)
            .WithMany(u => u.MeasurementsTaken)
            .HasForeignKey(vm => vm.MeasuredByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes
        builder.HasIndex(vm => vm.VisitId);
        builder.HasIndex(vm => vm.MeasurementDefinitionId);
        builder.HasIndex(vm => vm.MeasuredByUserId);
        builder.HasIndex(vm => vm.MeasurementDate);
        builder.HasIndex(vm => vm.CreatedAt);
        
        // Unique constraint - one measurement per visit per definition
        builder.HasIndex(vm => new { vm.VisitId, vm.MeasurementDefinitionId })
            .IsUnique();
    }
}