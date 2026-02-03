using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        
        // Primary key
        builder.HasKey(p => p.Id);
        
        // Properties
        builder.Property(p => p.PatientTransactionId).IsRequired();
        builder.Property(p => p.Amount)
            .HasPrecision(18, 2)
            .IsRequired();
        builder.Property(p => p.PaymentDate).IsRequired();
        builder.Property(p => p.Method).IsRequired();
        builder.Property(p => p.Notes)
            .HasMaxLength(500)
            .IsRequired(false);
        
        // Relationships
        builder.HasOne(p => p.PatientTransaction)
            .WithMany(pt => pt.Payments)
            .HasForeignKey(p => p.PatientTransactionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes
        builder.HasIndex(p => p.PatientTransactionId);
        builder.HasIndex(p => p.PaymentDate);
        builder.HasIndex(p => p.Method);
    }
}