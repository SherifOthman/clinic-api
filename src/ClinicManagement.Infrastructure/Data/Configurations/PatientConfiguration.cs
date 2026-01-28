using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");
        
        builder.Property(e => e.FullName)
            .HasMaxLength(200)
            .IsRequired();
        
        // Location fields
        builder.Property(e => e.Address)
            .HasMaxLength(500);
        builder.Property(e => e.GeoNameId);

        // Relationships
        builder.HasOne(p => p.Clinic)
            .WithMany(c => c.Patients)
            .HasForeignKey(p => p.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.PhoneNumbers)
            .WithOne(pn => pn.Patient)
            .HasForeignKey(pn => pn.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.ChronicDiseases)
            .WithOne(pcd => pcd.Patient)
            .HasForeignKey(pcd => pcd.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(p => p.FullName);
        builder.HasIndex(p => p.ClinicId);
        builder.HasIndex(p => p.GeoNameId);
    }
}

