using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");
        
        // Name fields configuration
        builder.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
        builder.Property(e => e.MiddleName).HasMaxLength(50);
        builder.Property(e => e.LastName).HasMaxLength(50).IsRequired();
        
        // Ignore computed property
        builder.Ignore(e => e.FullName);
        
        // Other properties
        builder.Property(e => e.Avatar).HasMaxLength(200);
        builder.Property(e => e.City).HasMaxLength(50);
        builder.Property(e => e.PhoneNumber).HasMaxLength(20);
        builder.Property(e => e.EmergencyContactName).HasMaxLength(50);
        builder.Property(e => e.EmergencyPhone).HasMaxLength(20);
        builder.Property(e => e.GeneralNotes).HasColumnType("nvarchar(max)");
        
        // Indexes for fast name searching
        builder.HasIndex(e => e.FirstName).HasDatabaseName("IX_Patients_FirstName");
        builder.HasIndex(e => e.LastName).HasDatabaseName("IX_Patients_LastName");
        builder.HasIndex(e => new { e.FirstName, e.LastName }).HasDatabaseName("IX_Patients_FirstName_LastName");
        
        // Relationships
        builder.HasOne(d => d.Clinic)
            .WithMany(p => p.Patients)
            .HasForeignKey(d => d.ClinicId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

