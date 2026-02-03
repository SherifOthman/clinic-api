using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class TransactionItemConfiguration : IEntityTypeConfiguration<TransactionItem>
{
    public void Configure(EntityTypeBuilder<TransactionItem> builder)
    {
        builder.ToTable("TransactionItems");
        
        // Primary key
        builder.HasKey(ti => ti.Id);
        
        // Properties
        builder.Property(ti => ti.PatientTransactionId).IsRequired();
        builder.Property(ti => ti.InventoryItemId).IsRequired();
        builder.Property(ti => ti.Quantity).IsRequired();
        builder.Property(ti => ti.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();
        
        // Relationships
        builder.HasOne(ti => ti.PatientTransaction)
            .WithMany(pt => pt.Items)
            .HasForeignKey(ti => ti.PatientTransactionId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(ti => ti.InventoryItem)
            .WithMany(ii => ii.TransactionItems)
            .HasForeignKey(ti => ti.InventoryItemId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes
        builder.HasIndex(ti => ti.PatientTransactionId);
        builder.HasIndex(ti => ti.InventoryItemId);
    }
}