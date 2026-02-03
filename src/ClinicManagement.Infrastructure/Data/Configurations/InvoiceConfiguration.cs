using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(i => i.Discount)
            .HasPrecision(18, 2);

        builder.HasOne(i => i.Clinic)
            .WithMany(c => c.Invoices)
            .HasForeignKey(i => i.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.ClinicPatient)
            .WithMany(cp => cp.Invoices)
            .HasForeignKey(i => i.ClinicPatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.MedicalVisit)
            .WithMany(mv => mv.Invoices)
            .HasForeignKey(i => i.MedicalVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(i => i.Items)
            .WithOne(ii => ii.Invoice)
            .HasForeignKey(ii => ii.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.Payments)
            .WithOne(p => p.Invoice)
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore calculated properties
        builder.Ignore(i => i.FinalAmount);
        builder.Ignore(i => i.TotalPaid);
        builder.Ignore(i => i.RemainingAmount);
        builder.Ignore(i => i.IsFullyPaid);
        builder.Ignore(i => i.IsOverdue);
    }
}