using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class ClinicConfiguration : IEntityTypeConfiguration<Clinic>
{
    public void Configure(EntityTypeBuilder<Clinic> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.BillingEmail).HasMaxLength(256);

        builder.HasIndex(c => c.OwnerUserId).IsUnique();
        builder.HasIndex(c => c.Name);

        builder.HasOne(c => c.Owner)
            .WithMany()
            .HasForeignKey(c => c.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.SubscriptionPlan)
            .WithMany()
            .HasForeignKey(c => c.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        // Child relationships — FK configured from child side
        // ClinicBranch.Clinic nav configured in ClinicBranchConfiguration
        builder.HasMany<ClinicBranch>()
            .WithOne(cb => cb.Clinic)
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

        // ClinicSubscription.SubscriptionPlan nav configured in ClinicSubscriptionConfiguration
        builder.HasMany<ClinicSubscription>()
            .WithOne()
            .HasForeignKey(cs => cs.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
