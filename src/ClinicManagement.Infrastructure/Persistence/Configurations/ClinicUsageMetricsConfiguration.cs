using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Persistence.Configurations;

public class ClinicUsageMetricsConfiguration : IEntityTypeConfiguration<ClinicUsageMetrics>
{
    public void Configure(EntityTypeBuilder<ClinicUsageMetrics> builder)
    {
        builder.Property(cum => cum.StorageUsedGB)
            .HasPrecision(18, 2);
    }
}
