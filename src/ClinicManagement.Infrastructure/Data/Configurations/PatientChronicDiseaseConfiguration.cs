using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class PatientChronicDiseaseConfiguration : IEntityTypeConfiguration<PatientChronicDisease>
{
    public void Configure(EntityTypeBuilder<PatientChronicDisease> builder)
    {
        builder.ToTable("PatientChronicDiseases");
        
        builder.Property(e => e.Notes).HasMaxLength(1000);

        // Relationships
        builder.HasOne(pcd => pcd.Patient)
            .WithMany(p => p.ChronicDiseases)
            .HasForeignKey(pcd => pcd.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pcd => pcd.ChronicDisease)
            .WithMany(cd => cd.PatientChronicDiseases)
            .HasForeignKey(pcd => pcd.ChronicDiseaseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(pcd => pcd.PatientId);
        builder.HasIndex(pcd => pcd.ChronicDiseaseId);
        builder.HasIndex(pcd => new { pcd.PatientId, pcd.ChronicDiseaseId }).IsUnique();
    }
}