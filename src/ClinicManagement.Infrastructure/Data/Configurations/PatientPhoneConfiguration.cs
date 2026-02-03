using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class PatientPhoneConfiguration : IEntityTypeConfiguration<PatientPhone>
{
    public void Configure(EntityTypeBuilder<PatientPhone> builder)
    {
        builder.ToTable("PatientPhones");
        
        // Primary key
        builder.HasKey(pp => pp.Id);
        
        // Properties
        builder.Property(pp => pp.ClinicPatientId).IsRequired();
        builder.Property(pp => pp.PhoneNumber)
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(pp => pp.Label)
            .HasMaxLength(50)
            .IsRequired(false);
        
        // Relationships
        builder.HasOne(pp => pp.ClinicPatient)
            .WithMany(cp => cp.PhoneNumbers)
            .HasForeignKey(pp => pp.ClinicPatientId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes
        builder.HasIndex(pp => pp.ClinicPatientId);
        builder.HasIndex(pp => pp.PhoneNumber);
    }
}