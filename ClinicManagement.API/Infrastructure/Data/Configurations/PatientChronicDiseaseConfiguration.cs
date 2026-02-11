using ClinicManagement.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.API.Infrastructure.Data.Configurations;

public class PatientChronicDiseaseConfiguration : IEntityTypeConfiguration<PatientChronicDisease>
{
    public void Configure(EntityTypeBuilder<PatientChronicDisease> builder)
    {
        builder.ToTable("PatientChronicDiseases");

        builder.HasKey(pcd => pcd.Id);

        // Foreign key relationships
        builder.HasOne(pcd => pcd.Patient)
            .WithMany(p => p.ChronicDiseases)
            .HasForeignKey(pcd => pcd.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pcd => pcd.ChronicDisease)
            .WithMany(cd => cd.Patients)
            .HasForeignKey(pcd => pcd.ChronicDiseaseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(pcd => new { pcd.PatientId, pcd.ChronicDiseaseId })
            .IsUnique()
            .HasDatabaseName("IX_PatientChronicDiseases_PatientId_ChronicDiseaseId");

        builder.HasIndex(pcd => pcd.PatientId);
        builder.HasIndex(pcd => pcd.ChronicDiseaseId);
    }
}
