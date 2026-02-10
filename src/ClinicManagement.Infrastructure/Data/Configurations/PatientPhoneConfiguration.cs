using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class PatientPhoneConfiguration : IEntityTypeConfiguration<PatientPhone>
{
    public void Configure(EntityTypeBuilder<PatientPhone> builder)
    {
        builder.ToTable("PatientPhones");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(p => p.IsPrimary)
            .IsRequired();

        builder.HasOne(p => p.Patient)
            .WithMany(pt => pt.PhoneNumbers)
            .HasForeignKey(p => p.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.PhoneNumber);
    }
}
