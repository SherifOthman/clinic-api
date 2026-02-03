using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class VisitConfiguration : IEntityTypeConfiguration<Visit>
{
    public void Configure(EntityTypeBuilder<Visit> builder)
    {
        builder.ToTable("Visits");
        
        // Primary key
        builder.HasKey(v => v.Id);
        
        // Properties
        builder.Property(v => v.ClinicId).IsRequired();
        builder.Property(v => v.ClinicPatientId).IsRequired();
        builder.Property(v => v.DoctorId).IsRequired();
        builder.Property(v => v.VisitDate).IsRequired();
        builder.Property(v => v.Diagnosis)
            .HasMaxLength(1000)
            .IsRequired(false);
        builder.Property(v => v.Notes)
            .HasMaxLength(2000)
            .IsRequired(false);
        builder.Property(v => v.Prescription)
            .HasMaxLength(2000)
            .IsRequired(false);
        
        // Relationships
        builder.HasOne(v => v.Clinic)
            .WithMany(c => c.Visits)
            .HasForeignKey(v => v.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(v => v.ClinicPatient)
            .WithMany(cp => cp.Visits)
            .HasForeignKey(v => v.ClinicPatientId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(v => v.Doctor)
            .WithMany(u => u.VisitsAsDoctor)
            .HasForeignKey(v => v.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Collections
        builder.HasMany(v => v.Transactions)
            .WithOne(pt => pt.Visit)
            .HasForeignKey(pt => pt.VisitId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(v => v.Measurements)
            .WithOne(vm => vm.Visit)
            .HasForeignKey(vm => vm.VisitId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes
        builder.HasIndex(v => v.ClinicId);
        builder.HasIndex(v => v.ClinicPatientId);
        builder.HasIndex(v => v.DoctorId);
        builder.HasIndex(v => v.VisitDate);
        builder.HasIndex(v => v.CreatedAt);
    }
}