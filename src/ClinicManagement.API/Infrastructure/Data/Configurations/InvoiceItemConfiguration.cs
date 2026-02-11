using ClinicManagement.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.API.Infrastructure.Data.Configurations;

public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.HasKey(ii => ii.Id);

        builder.Property(ii => ii.UnitPrice)
            .HasPrecision(18, 2);

        builder.HasOne(ii => ii.Invoice)
            .WithMany(i => i.Items)
            .HasForeignKey(ii => ii.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ii => ii.MedicalService)
            .WithMany(ms => ms.InvoiceItems)
            .HasForeignKey(ii => ii.MedicalServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ii => ii.Medicine)
            .WithMany(m => m.InvoiceItems)
            .HasForeignKey(ii => ii.MedicineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ii => ii.MedicalSupply)
            .WithMany(ms => ms.InvoiceItems)
            .HasForeignKey(ii => ii.MedicalSupplyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignore calculated properties
        builder.Ignore(ii => ii.LineTotal);
    }
}
