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
        builder.Property(p => p.PaymentMethod).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasIndex(p => p.InvoiceId);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Payment_Amount",  "[Amount] > 0");
            t.HasCheckConstraint("CK_Payment_Method",  "[PaymentMethod] IN ('Cash', 'CreditCard', 'DebitCard', 'BankTransfer', 'Check', 'DigitalWallet')");
            t.HasCheckConstraint("CK_Payment_Status",  "[Status] IN ('Unpaid', 'PartiallyPaid', 'Paid')");
        });
    }
}
