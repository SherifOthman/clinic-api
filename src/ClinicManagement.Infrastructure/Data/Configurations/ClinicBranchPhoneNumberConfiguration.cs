using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class ClinicBranchPhoneNumberConfiguration : IEntityTypeConfiguration<ClinicBranchPhoneNumber>
{
    public void Configure(EntityTypeBuilder<ClinicBranchPhoneNumber> builder)
    {
        builder.ToTable("ClinicBranchPhoneNumbers");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PhoneNumber)
            .IsRequired()
            .HasMaxLength(15);

        builder.Property(p => p.Label)
            .HasMaxLength(30);

        builder.HasOne(p => p.ClinicBranch)
            .WithMany(cb => cb.PhoneNumbers)
            .HasForeignKey(p => p.ClinicBranchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.PhoneNumber);
    }
}
