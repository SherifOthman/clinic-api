using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class PatientPhoneConfiguration : IEntityTypeConfiguration<PatientPhone>
{
    public void Configure(EntityTypeBuilder<PatientPhone> builder)
    {
        builder.Property(pp => pp.PhoneNumber).HasMaxLength(20).IsRequired();
        builder.Property(pp => pp.NationalNumber).HasMaxLength(15).IsRequired().HasDefaultValue("");
        builder.HasIndex(pp => pp.PhoneNumber);
        builder.HasIndex(pp => pp.NationalNumber);
    }
}
