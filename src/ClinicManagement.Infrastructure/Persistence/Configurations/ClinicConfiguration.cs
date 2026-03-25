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
        
        builder.HasOne(c => c.Owner)
            .WithMany()
            .HasForeignKey(c => c.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(c => c.SubscriptionPlan)
            .WithMany()
            .HasForeignKey(c => c.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(c => c.Branches)
            .WithOne(cb => cb.Clinic)
            .HasForeignKey(cb => cb.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(c => c.Staff)
            .WithOne(s => s.Clinic)
            .HasForeignKey(s => s.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(c => c.Patients)
            .WithOne(p => p.Clinic)
            .HasForeignKey(p => p.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(c => c.Subscriptions)
            .WithOne()
            .HasForeignKey(cs => cs.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
