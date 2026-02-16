using ClinicManagement.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.API.Infrastructure.Data.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PatientCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(p => p.FullName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.CityGeoNameId)
            .IsRequired(false);

        builder.Property(p => p.DateOfBirth)
            .IsRequired();

        // Ignore calculated properties (not stored in database)
        builder.Ignore(p => p.Age);
        builder.Ignore(p => p.IsAdult);
        builder.Ignore(p => p.IsMinor);
        builder.Ignore(p => p.IsSenior);
        builder.Ignore(p => p.PrimaryPhoneNumber);
        builder.Ignore(p => p.HasChronicDiseases);
        builder.Ignore(p => p.ChronicDiseaseCount);

        builder.HasOne(p => p.Clinic)
            .WithMany()
            .HasForeignKey(p => p.ClinicId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(p => p.PatientCode);
        builder.HasIndex(p => p.ClinicId);
        builder.HasIndex(p => p.CityGeoNameId);
    }
}
