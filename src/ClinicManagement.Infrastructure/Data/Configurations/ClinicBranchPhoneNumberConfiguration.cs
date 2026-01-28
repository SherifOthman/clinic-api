using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class ClinicBranchPhoneNumberConfiguration : IEntityTypeConfiguration<ClinicBranchPhoneNumber>
{
    public void Configure(EntityTypeBuilder<ClinicBranchPhoneNumber> builder)
    {
        builder.ToTable("ClinicBranchPhoneNumbers");
        
        builder.Property(e => e.PhoneNumber).HasMaxLength(20).IsRequired();
        builder.Property(e => e.Label).HasMaxLength(50);

        // Relationships
        builder.HasOne(pn => pn.ClinicBranch)
            .WithMany(cb => cb.PhoneNumbers)
            .HasForeignKey(pn => pn.ClinicBranchId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(pn => pn.ClinicBranchId);
        builder.HasIndex(pn => pn.PhoneNumber);
    }
}