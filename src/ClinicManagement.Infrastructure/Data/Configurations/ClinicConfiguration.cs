using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class ClinicConfiguration : IEntityTypeConfiguration<Clinic>
{
    public void Configure(EntityTypeBuilder<Clinic> builder)
    {
        builder.ToTable("Clinics");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(c => c.Owner)
            .WithMany(u => u.OwnedClinics)
            .HasForeignKey(c => c.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.SubscriptionPlan)
            .WithMany()
            .HasForeignKey(c => c.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Staff)
            .WithOne(s => s.Clinic)
            .HasForeignKey(s => s.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(c => c.Name);
        builder.HasIndex(c => c.OwnerUserId);
    }
}
