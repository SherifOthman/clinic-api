using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.Property(ii => ii.UnitPrice).HasPrecision(18, 2);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_InvoiceItem_UnitPrice", "[UnitPrice] >= 0");
            t.HasCheckConstraint("CK_InvoiceItem_Quantity",  "[Quantity] > 0");
        });
    }
}
