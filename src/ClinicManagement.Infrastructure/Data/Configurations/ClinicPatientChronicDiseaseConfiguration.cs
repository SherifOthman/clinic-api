using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class ClinicPatientChronicDiseaseConfiguration : IEntityTypeConfiguration<ClinicPatientChronicDisease>
{
    public void Configure(EntityTypeBuilder<ClinicPatientChronicDisease> builder)
    {
        builder.ToTable("ClinicPatientChronicDiseases");

        builder.HasKey(cpcd => cpcd.Id);

        // Foreign key relationships
        builder.HasOne(cpcd => cpcd.ClinicPatient)
            .WithMany(cp => cp.ChronicDiseases)
            .HasForeignKey(cpcd => cpcd.ClinicPatientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cpcd => cpcd.ChronicDisease)
            .WithMany(cd => cd.ClinicPatients)
            .HasForeignKey(cpcd => cpcd.ChronicDiseaseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Properties
        builder.Property(cpcd => cpcd.DiagnosedDate)
            .HasColumnType("date");

        builder.Property(cpcd => cpcd.Status)
            .HasMaxLength(50);

        builder.Property(cpcd => cpcd.Notes)
            .HasMaxLength(1000);

        builder.Property(cpcd => cpcd.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Indexes
        builder.HasIndex(cpcd => new { cpcd.ClinicPatientId, cpcd.ChronicDiseaseId })
            .IsUnique()
            .HasDatabaseName("IX_ClinicPatientChronicDiseases_ClinicPatientId_ChronicDiseaseId");

        builder.HasIndex(cpcd => cpcd.ClinicPatientId);
        builder.HasIndex(cpcd => cpcd.ChronicDiseaseId);
        builder.HasIndex(cpcd => cpcd.IsActive);
    }
}
