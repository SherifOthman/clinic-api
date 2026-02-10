using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class InvoiceConfiguration : BaseEntityConfiguration<Invoice>
{
    public override void Configure(EntityTypeBuilder<Invoice> builder)
    {
        base.Configure(builder);

        builder.Property(i => i.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(i => i.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(i => i.Discount)
            .HasPrecision(18, 2);

        builder.Property(i => i.TaxAmount)
            .HasPrecision(18, 2);

        builder.HasOne(i => i.Clinic)
            .WithMany(c => c.Invoices)
            .HasForeignKey(i => i.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Patient)
            .WithMany(cp => cp.Invoices)
            .HasForeignKey(i => i.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.MedicalVisit)
            .WithMany(mv => mv.Invoices)
            .HasForeignKey(i => i.MedicalVisitId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(i => i.Items)
            .WithOne(ii => ii.Invoice)
            .HasForeignKey(ii => ii.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.Payments)
            .WithOne(p => p.Invoice)
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint on invoice number per clinic
        builder.HasIndex(i => new { i.ClinicId, i.InvoiceNumber })
            .IsUnique();

        // Ignore calculated properties
        builder.Ignore(i => i.FinalAmount);
        builder.Ignore(i => i.TotalPaid);
        builder.Ignore(i => i.RemainingAmount);
        builder.Ignore(i => i.IsFullyPaid);
        builder.Ignore(i => i.IsOverdue);
    }
}
