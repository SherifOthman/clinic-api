using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("InventoryItems");
        
        // Primary key
        builder.HasKey(ii => ii.Id);
        
        // Properties
        builder.Property(ii => ii.ClinicId).IsRequired();
        builder.Property(ii => ii.Name)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(ii => ii.Description)
            .HasMaxLength(1000)
            .IsRequired(false);
        builder.Property(ii => ii.Type).IsRequired();
        builder.Property(ii => ii.Unit)
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(ii => ii.CurrentStock)
            .HasPrecision(18, 2)
            .IsRequired(false);
        builder.Property(ii => ii.MinimumStock)
            .HasPrecision(18, 2)
            .IsRequired(false);
        builder.Property(ii => ii.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired(false);
        builder.Property(ii => ii.IsActive).IsRequired();
        
        // Relationships
        builder.HasOne(ii => ii.Clinic)
            .WithMany(c => c.InventoryItems)
            .HasForeignKey(ii => ii.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Collections
        builder.HasMany(ii => ii.TransactionItems)
            .WithOne(ti => ti.InventoryItem)
            .HasForeignKey(ti => ti.InventoryItemId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes
        builder.HasIndex(ii => ii.ClinicId);
        builder.HasIndex(ii => ii.Name);
        builder.HasIndex(ii => ii.Type);
        builder.HasIndex(ii => ii.IsActive);
    }
}