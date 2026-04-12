using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class ClinicBranchPhoneNumberConfiguration : IEntityTypeConfiguration<ClinicBranchPhoneNumber>
{
    public void Configure(EntityTypeBuilder<ClinicBranchPhoneNumber> builder)
    {
        builder.Property(p => p.PhoneNumber).HasMaxLength(20).IsRequired();
        builder.Property(p => p.Label).HasMaxLength(50);
    }
}
