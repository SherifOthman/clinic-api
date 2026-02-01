using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class ChronicDiseaseConfiguration : IEntityTypeConfiguration<ChronicDisease>
{
    public void Configure(EntityTypeBuilder<ChronicDisease> builder)
    {
        builder.ToTable("ChronicDiseases");
        
        builder.Property(e => e.NameEn).HasMaxLength(100).IsRequired();
        builder.Property(e => e.NameAr).HasMaxLength(100).IsRequired();
        builder.Property(e => e.DescriptionEn).HasMaxLength(500);
        builder.Property(e => e.DescriptionAr).HasMaxLength(500);

        // Relationships
        builder.HasMany(cd => cd.PatientChronicDiseases)
            .WithOne(pcd => pcd.ChronicDisease)
            .HasForeignKey(pcd => pcd.ChronicDiseaseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(cd => cd.NameEn);
        builder.HasIndex(cd => cd.NameAr);
        builder.HasIndex(cd => cd.IsActive);
    }
}