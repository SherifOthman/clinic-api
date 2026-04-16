using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.Property(sp => sp.Name).HasMaxLength(100).IsRequired();
        builder.Property(sp => sp.NameAr).HasMaxLength(100).IsRequired();
        builder.Property(sp => sp.Description).HasMaxLength(500);
        builder.Property(sp => sp.DescriptionAr).HasMaxLength(500);
        builder.Property(sp => sp.MonthlyFee).HasPrecision(18, 2);
        builder.Property(sp => sp.YearlyFee).HasPrecision(18, 2);
        builder.Property(sp => sp.SetupFee).HasPrecision(18, 2);
    }
}
