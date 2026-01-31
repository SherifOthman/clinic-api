using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("SubscriptionPlans");
        
        builder.Property(e => e.Name).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.Price).HasColumnType("decimal(18,2)");
        
        // Default values for new fields
        builder.Property(e => e.MaxClinics).HasDefaultValue(1);
        builder.Property(e => e.MaxBranches).HasDefaultValue(1);
        builder.Property(e => e.HasAdvancedReporting).HasDefaultValue(false);
        builder.Property(e => e.HasApiAccess).HasDefaultValue(false);
        builder.Property(e => e.HasPrioritySupport).HasDefaultValue(false);
        builder.Property(e => e.HasCustomBranding).HasDefaultValue(false);

        // Indexes
        builder.HasIndex(s => s.Name);
        builder.HasIndex(s => s.IsActive);
    }
}