using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.Property(p => p.PatientCode).HasMaxLength(50).IsRequired();
        builder.Property(p => p.BloodType).HasConversion<string>().HasMaxLength(15);

        builder.ToTable(t => t.HasCheckConstraint("CK_Patient_BloodType",
            "[BloodType] IS NULL OR [BloodType] IN ('APositive', 'ANegative', 'BPositive', 'BNegative', 'ABPositive', 'ABNegative', 'OPositive', 'ONegative')"));

        // PersonId — required, every patient must have a Person
        builder.Property(p => p.PersonId).IsRequired();
        builder.HasOne(p => p.Person)
            .WithMany(per => per.PatientRecords)
            .HasForeignKey(p => p.PersonId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

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

        // PatientCode is unique per clinic (not globally) — same code can exist in different clinics
        builder.HasIndex(p => new { p.ClinicId, p.PatientCode }).IsUnique();
        builder.HasIndex(p => new { p.ClinicId, p.IsDeleted, p.CreatedAt });
        builder.HasIndex(p => p.StateGeonameId);
        builder.HasIndex(p => p.CityGeonameId);
        builder.HasIndex(p => p.CountryGeonameId);

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
