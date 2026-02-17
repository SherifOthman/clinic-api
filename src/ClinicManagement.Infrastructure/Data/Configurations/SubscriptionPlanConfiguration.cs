using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("SubscriptionPlans");

        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.Name)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(sp => sp.Description)
            .HasMaxLength(255);

        builder.Property(sp => sp.MonthlyFee)
            .HasPrecision(18, 2);

        builder.Property(sp => sp.YearlyFee)
            .HasPrecision(18, 2);

        builder.Property(sp => sp.SetupFee)
            .HasPrecision(18, 2);

        builder.HasIndex(sp => sp.Name)
            .IsUnique();
    }
}
