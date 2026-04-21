using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.Property(ii => ii.UnitPrice).HasPrecision(18, 2);
        builder.Property(ii => ii.SaleUnit).HasConversion<string>().HasMaxLength(10);

        // Index FK columns — SQL Server does NOT auto-index FKs
        builder.HasIndex(ii => ii.InvoiceId);
        builder.HasIndex(ii => ii.MedicalServiceId);
        builder.HasIndex(ii => ii.MedicineId);
        builder.HasIndex(ii => ii.MedicalSupplyId);
        builder.HasIndex(ii => ii.MedicineDispensingId);
        builder.HasIndex(ii => ii.LabTestOrderId);
        builder.HasIndex(ii => ii.RadiologyOrderId);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_InvoiceItem_UnitPrice", "[UnitPrice] >= 0");
            t.HasCheckConstraint("CK_InvoiceItem_Quantity",  "[Quantity] > 0");
            t.HasCheckConstraint("CK_InvoiceItem_SaleUnit",  "[SaleUnit] IS NULL OR [SaleUnit] IN ('Box', 'Strip')");

            // Exactly one item source FK must be set — enforced at DB level
            t.HasCheckConstraint("CK_InvoiceItem_ExactlyOneSource",
                @"(CASE WHEN [MedicalServiceId]     IS NOT NULL THEN 1 ELSE 0 END +
                   CASE WHEN [MedicineId]            IS NOT NULL THEN 1 ELSE 0 END +
                   CASE WHEN [MedicalSupplyId]       IS NOT NULL THEN 1 ELSE 0 END +
                   CASE WHEN [MedicineDispensingId]  IS NOT NULL THEN 1 ELSE 0 END +
                   CASE WHEN [LabTestOrderId]        IS NOT NULL THEN 1 ELSE 0 END +
                   CASE WHEN [RadiologyOrderId]      IS NOT NULL THEN 1 ELSE 0 END) = 1");
        });
    }
}

