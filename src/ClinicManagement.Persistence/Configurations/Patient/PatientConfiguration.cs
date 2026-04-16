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
        builder.HasIndex(p => p.StateGeonameId);
        builder.HasIndex(p => p.CityGeonameId);
        builder.HasIndex(p => p.CountryGeonameId);
        builder.HasIndex(p => p.Gender);

        // Optional FK relationships to GeoNames tables — no cascade delete (geo data is shared)
        // NoAction required: SQL Server rejects multiple SetNull cascade paths on the same table
        builder.HasOne(p => p.Country)
            .WithMany()
            .HasForeignKey(p => p.CountryGeonameId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(p => p.State)
            .WithMany()
            .HasForeignKey(p => p.StateGeonameId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(p => p.City)
            .WithMany()
            .HasForeignKey(p => p.CityGeonameId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
