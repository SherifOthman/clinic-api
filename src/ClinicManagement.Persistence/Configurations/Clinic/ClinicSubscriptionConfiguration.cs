using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class ClinicSubscriptionConfiguration : IEntityTypeConfiguration<ClinicSubscription>
{
    public void Configure(EntityTypeBuilder<ClinicSubscription> builder)
    {
        builder.Property(cs => cs.Status).HasConversion<short>().HasColumnType("smallint");
        builder.Property(cs => cs.CancellationReason).HasMaxLength(500);

        builder.HasIndex(cs => new { cs.ClinicId, cs.Status });

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_ClinicSubscription_Dates",  "[EndDate] IS NULL OR [EndDate] > [StartDate]");
            t.HasCheckConstraint("CK_ClinicSubscription_Status", "[Status] IN (0, 1, 2, 3, 4)");
        });

        builder.HasOne(cs => cs.SubscriptionPlan)
            .WithMany()
            .HasForeignKey(cs => cs.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
