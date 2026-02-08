using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PatientCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.CityGeoNameId)
            .IsRequired(false);

        builder.Property(p => p.DateOfBirth)
            .IsRequired();

        builder.HasOne(p => p.Clinic)
            .WithMany()
            .HasForeignKey(p => p.ClinicId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(p => p.PatientCode);
        builder.HasIndex(p => p.ClinicId);
        builder.HasIndex(p => p.CityGeoNameId);
    }
}
