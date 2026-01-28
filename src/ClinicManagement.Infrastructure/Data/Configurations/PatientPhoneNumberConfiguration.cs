using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class PatientPhoneNumberConfiguration : IEntityTypeConfiguration<PatientPhoneNumber>
{
    public void Configure(EntityTypeBuilder<PatientPhoneNumber> builder)
    {
        builder.ToTable("PatientPhoneNumbers");
        
        builder.Property(e => e.PhoneNumber).HasMaxLength(20).IsRequired();

        // Relationships
        builder.HasOne(pn => pn.Patient)
            .WithMany(p => p.PhoneNumbers)
            .HasForeignKey(pn => pn.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(pn => pn.PatientId);
        builder.HasIndex(pn => pn.PhoneNumber);
    }
}