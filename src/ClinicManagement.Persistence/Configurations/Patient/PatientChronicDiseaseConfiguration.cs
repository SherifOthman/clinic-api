using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class PatientChronicDiseaseConfiguration : IEntityTypeConfiguration<PatientChronicDisease>
{
    public void Configure(EntityTypeBuilder<PatientChronicDisease> builder)
    {
        // ChronicDisease nav property — used in GetDetailAsync projection
        // p.ChronicDiseases.Select(cd => new { cd.ChronicDisease.NameEn, cd.ChronicDisease.NameAr })
        builder.HasOne(pcd => pcd.ChronicDisease)
            .WithMany()
            .HasForeignKey(pcd => pcd.ChronicDiseaseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
