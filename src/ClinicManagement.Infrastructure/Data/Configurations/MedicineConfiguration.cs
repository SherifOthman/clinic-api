using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class MedicineConfiguration : BaseEntityConfiguration<Medicine>
{
    public override void Configure(EntityTypeBuilder<Medicine> builder)
    {
        base.Configure(builder);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.BoxPrice)
            .HasPrecision(18, 2);

        builder.HasOne(m => m.ClinicBranch)
            .WithMany(cb => cb.Medicines)
            .HasForeignKey(m => m.ClinicBranchId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint: medicine name must be unique per branch
        builder.HasIndex(m => new { m.ClinicBranchId, m.Name })
            .IsUnique();
        
        // Ignore calculated properties (computed at runtime from stock and pricing)
        builder.Ignore(m => m.StripPrice);
        builder.Ignore(m => m.FullBoxesInStock);
        builder.Ignore(m => m.RemainingStrips);
        builder.Ignore(m => m.IsLowStock);
        builder.Ignore(m => m.NeedsReorder);
        builder.Ignore(m => m.HasStock);
        builder.Ignore(m => m.IsExpired);
        builder.Ignore(m => m.IsExpiringSoon);
        builder.Ignore(m => m.StockStatus);
        builder.Ignore(m => m.DaysUntilExpiry);
        builder.Ignore(m => m.InventoryValue);
    }
}
