using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class PatientTransactionConfiguration : IEntityTypeConfiguration<PatientTransaction>
{
    public void Configure(EntityTypeBuilder<PatientTransaction> builder)
    {
        builder.ToTable("PatientTransactions");
        
        // Primary key
        builder.HasKey(pt => pt.Id);
        
        // Properties
        builder.Property(pt => pt.ClinicId).IsRequired();
        builder.Property(pt => pt.ClinicPatientId).IsRequired();
        builder.Property(pt => pt.VisitId).IsRequired(false);
        
        // Relationships
        builder.HasOne(pt => pt.Clinic)
            .WithMany(c => c.Transactions)
            .HasForeignKey(pt => pt.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(pt => pt.ClinicPatient)
            .WithMany(cp => cp.Transactions)
            .HasForeignKey(pt => pt.ClinicPatientId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(pt => pt.Visit)
            .WithMany(v => v.Transactions)
            .HasForeignKey(pt => pt.VisitId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Collections
        builder.HasMany(pt => pt.Services)
            .WithOne(ts => ts.PatientTransaction)
            .HasForeignKey(ts => ts.PatientTransactionId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(pt => pt.Items)
            .WithOne(ti => ti.PatientTransaction)
            .HasForeignKey(ti => ti.PatientTransactionId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(pt => pt.Payments)
            .WithOne(p => p.PatientTransaction)
            .HasForeignKey(p => p.PatientTransactionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes
        builder.HasIndex(pt => pt.ClinicId);
        builder.HasIndex(pt => pt.ClinicPatientId);
        builder.HasIndex(pt => pt.VisitId);
        builder.HasIndex(pt => pt.CreatedAt);
        
        // Ignore calculated properties - use Domain Service instead
        // IPatientTransactionDomainService.CalculateTotalAmount()
        // IPatientTransactionDomainService.CalculatePaidAmount()
        // IPatientTransactionDomainService.CalculateRemainingAmount()
    }
}