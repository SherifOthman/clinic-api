using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class SpecializationConfiguration : IEntityTypeConfiguration<Specialization>
{
    public void Configure(EntityTypeBuilder<Specialization> builder)
    {
        builder.ToTable("Specializations");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.NameEn)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.NameAr)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.DescriptionEn)
            .HasMaxLength(500);

        builder.Property(s => s.DescriptionAr)
            .HasMaxLength(500);

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Index for performance
        builder.HasIndex(s => s.NameEn);
        builder.HasIndex(s => s.NameAr);
        builder.HasIndex(s => s.IsActive);

        // Note: Seed data is now handled by ComprehensiveSeedService
        // Removed static seed data to avoid conflicts
    }
}
