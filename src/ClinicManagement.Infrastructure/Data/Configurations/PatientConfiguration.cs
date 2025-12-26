using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");
        
        builder.Property(e => e.Name).HasMaxLength(100).IsRequired();
        builder.Property(e => e.PhoneNumber).HasMaxLength(20);
        
        builder.HasOne(d => d.Clinic)
            .WithMany(p => p.Patients)
            .HasForeignKey(d => d.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Appointments)
            .WithOne(a => a.Patient)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(p => p.ClinicId);
        builder.HasIndex(p => p.Name);
    }
}

