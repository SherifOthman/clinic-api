using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class ClinicSubscriptionConfiguration : IEntityTypeConfiguration<ClinicSubscription>
{
    public void Configure(EntityTypeBuilder<ClinicSubscription> builder)
    {
        // SubscriptionPlan nav — used in GetLatestAsync to get plan name
        // s.SubscriptionPlan.Name instead of correlated subquery
        builder.HasOne(cs => cs.SubscriptionPlan)
            .WithMany()
            .HasForeignKey(cs => cs.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
