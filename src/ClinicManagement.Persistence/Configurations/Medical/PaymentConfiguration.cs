using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.Property(p => p.Amount).HasPrecision(18, 2);
        builder.Property(p => p.Note).HasMaxLength(500);
        builder.Property(p => p.ReferenceNumber).HasMaxLength(100);
        builder.Property(p => p.PaymentMethod).HasConversion<short>().HasColumnType("smallint");
        builder.Property(p => p.Status).HasConversion<short>().HasColumnType("smallint");

        builder.HasIndex(p => p.InvoiceId);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Payment_Amount",  "[Amount] > 0");
            t.HasCheckConstraint("CK_Payment_Method",  "[PaymentMethod] IN (1, 2, 3, 4, 5, 6)");
            t.HasCheckConstraint("CK_Payment_Status",  "[Status] IN (0, 1, 2)");
        });
    }
}
