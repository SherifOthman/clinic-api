using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Persistence.Configurations;

public class ClinicBranchAppointmentPriceConfiguration : IEntityTypeConfiguration<ClinicBranchAppointmentPrice>
{
    public void Configure(EntityTypeBuilder<ClinicBranchAppointmentPrice> builder)
    {
        builder.Property(cbap => cbap.Price)
            .HasPrecision(18, 2);
    }
}
