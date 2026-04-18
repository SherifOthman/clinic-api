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

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_SubscriptionPlan_MonthlyFee",  "[MonthlyFee] >= 0");
            t.HasCheckConstraint("CK_SubscriptionPlan_YearlyFee",   "[YearlyFee] >= 0");
            t.HasCheckConstraint("CK_SubscriptionPlan_SetupFee",    "[SetupFee] >= 0");
            t.HasCheckConstraint("CK_SubscriptionPlan_MaxStaff",    "[MaxStaff] > 0");
            t.HasCheckConstraint("CK_SubscriptionPlan_MaxBranches", "[MaxBranches] > 0");
        });
    }
}
