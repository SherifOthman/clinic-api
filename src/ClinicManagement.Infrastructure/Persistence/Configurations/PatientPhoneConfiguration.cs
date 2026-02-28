using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Persistence.Configurations;

public class PatientPhoneConfiguration : IEntityTypeConfiguration<PatientPhone>
{
    public void Configure(EntityTypeBuilder<PatientPhone> builder)
    {
        builder.Property(pp => pp.PhoneNumber).HasMaxLength(20).IsRequired();
    }
}
