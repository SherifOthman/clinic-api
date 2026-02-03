using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class TransactionServiceConfiguration : IEntityTypeConfiguration<TransactionService>
{
    public void Configure(EntityTypeBuilder<TransactionService> builder)
    {
        builder.ToTable("TransactionServices");
        
        // Primary key
        builder.HasKey(ts => ts.Id);
        
        // Properties
        builder.Property(ts => ts.PatientTransactionId).IsRequired();
        builder.Property(ts => ts.ServiceId).IsRequired();
        builder.Property(ts => ts.Price)
            .HasPrecision(18, 2)
            .IsRequired();
        builder.Property(ts => ts.Type).IsRequired();
        
        // Relationships
        builder.HasOne(ts => ts.PatientTransaction)
            .WithMany(pt => pt.Services)
            .HasForeignKey(ts => ts.PatientTransactionId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(ts => ts.Service)
            .WithMany(ms => ms.TransactionServices)
            .HasForeignKey(ts => ts.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes
        builder.HasIndex(ts => ts.PatientTransactionId);
        builder.HasIndex(ts => ts.ServiceId);
        builder.HasIndex(ts => ts.Type);
    }
}