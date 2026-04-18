using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class SubscriptionPaymentConfiguration : IEntityTypeConfiguration<SubscriptionPayment>
{
    public void Configure(EntityTypeBuilder<SubscriptionPayment> builder)
    {
        builder.Property(sp => sp.Amount).HasPrecision(18, 2);
        builder.Property(sp => sp.RefundAmount).HasPrecision(18, 2);
        builder.Property(sp => sp.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.ToTable(t => t.HasCheckConstraint("CK_SubscriptionPayment_Status",
            "[Status] IN ('Pending', 'Completed', 'Failed', 'Refunded')"));
    }
}
