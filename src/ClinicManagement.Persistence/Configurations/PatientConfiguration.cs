using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.Property(p => p.PatientCode).HasMaxLength(50).IsRequired();
        builder.Property(p => p.FullName).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Gender)
            .HasConversion<short>()
            .HasColumnType("smallint");

        builder.HasOne<Clinic>()
            .WithMany()
            .HasForeignKey(p => p.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Phones)
            .WithOne()
            .HasForeignKey(pp => pp.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        // ChronicDiseases is a join table — configure the relationship to ChronicDisease
        builder.HasMany(p => p.ChronicDiseases)
            .WithOne()
            .HasForeignKey(pcd => pcd.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.PatientCode).IsUnique();
        builder.HasIndex(p => new { p.ClinicId, p.IsDeleted, p.CreatedAt });
        builder.HasIndex(p => p.FullName);
        builder.HasIndex(p => new { p.StateNameEn, p.StateNameAr });
        builder.HasIndex(p => new { p.CityNameEn, p.CityNameAr });
        builder.HasIndex(p => new { p.CountryNameEn, p.CountryNameAr });
        builder.HasIndex(p => p.Gender);
    }
}
