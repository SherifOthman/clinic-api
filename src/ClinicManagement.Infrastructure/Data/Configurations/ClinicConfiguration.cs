using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class ClinicConfiguration : IEntityTypeConfiguration<Clinic>
{
    public void Configure(EntityTypeBuilder<Clinic> builder)
    {
        builder.ToTable("Clinics");
        builder.Property(e => e.Name).HasMaxLength(100);
        builder.Property(e => e.Phone).HasMaxLength(20);
        builder.Property(e => e.StartDate).HasDefaultValueSql("GETDATE()");
        builder.HasOne(d => d.Owner)
            .WithMany()
            .HasForeignKey(d => d.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(d => d.SubscriptionPlan)
            .WithMany(p => p.Clinics)
            .HasForeignKey(d => d.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

