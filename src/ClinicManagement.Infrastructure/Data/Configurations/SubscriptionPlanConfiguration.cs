using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("SubscriptionPlans");
        builder.Property(e => e.Name).HasMaxLength(50);
        builder.Property(e => e.Price).HasColumnType("decimal(10,2)");
        builder.Property(e => e.Features).HasColumnType("nvarchar(max)");
    }
}

