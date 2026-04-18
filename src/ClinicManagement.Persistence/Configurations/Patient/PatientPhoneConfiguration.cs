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

        // A patient cannot have the same phone number twice
        builder.HasIndex(pp => new { pp.PatientId, pp.PhoneNumber }).IsUnique();
        builder.HasIndex(pp => pp.NationalNumber);
    }
}
