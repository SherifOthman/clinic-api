using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Persistence.Configurations;

public class ClinicConfiguration : IEntityTypeConfiguration<Clinic>
{
    public void Configure(EntityTypeBuilder<Clinic> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.BillingEmail).HasMaxLength(256);

        // Nav properties kept — Owner and SubscriptionPlan are traversed in handlers
        builder.HasOne(c => c.Owner)
            .WithMany()
            .HasForeignKey(c => c.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.SubscriptionPlan)
            .WithMany()
            .HasForeignKey(c => c.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        // Collection nav properties removed from Clinic — configure FK only from child side
        builder.HasMany<ClinicBranch>()
            .WithOne()
            .HasForeignKey(cb => cb.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<Staff>()
            .WithOne()
            .HasForeignKey(s => s.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<Patient>()
            .WithOne()
            .HasForeignKey(p => p.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<ClinicSubscription>()
            .WithOne()
            .HasForeignKey(cs => cs.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
