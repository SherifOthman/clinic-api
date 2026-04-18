using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.Property(i => i.InvoiceNumber).HasMaxLength(50).IsRequired();
        builder.Property(i => i.Notes).HasMaxLength(1000);
        builder.Property(i => i.TotalAmount).HasPrecision(18, 2);
        builder.Property(i => i.Discount).HasPrecision(18, 2);
        builder.Property(i => i.TaxAmount).HasPrecision(18, 2);
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasIndex(i => new { i.ClinicId, i.InvoiceNumber }).IsUnique();
        builder.HasIndex(i => new { i.PatientId, i.Status });

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Invoice_TotalAmount", "[TotalAmount] >= 0");
            t.HasCheckConstraint("CK_Invoice_Discount",    "[Discount] >= 0");
            t.HasCheckConstraint("CK_Invoice_TaxAmount",   "[TaxAmount] >= 0");
            t.HasCheckConstraint("CK_Invoice_DueDate",     "[DueDate] IS NULL OR [IssuedDate] IS NULL OR [DueDate] >= [IssuedDate]");
            t.HasCheckConstraint("CK_Invoice_Status",      "[Status] IN ('Draft', 'Issued', 'PartiallyPaid', 'FullyPaid', 'Cancelled', 'Overdue')");
        });
    }
}
