using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");
        builder.Property(e => e.Avatar).HasMaxLength(200);
        builder.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.SecondName).HasMaxLength(100);
        builder.Property(e => e.ThirdName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.City).HasMaxLength(50);
        builder.Property(e => e.PhoneNumber).HasMaxLength(20);
        builder.Property(e => e.EmergencyContactName).HasMaxLength(50);
        builder.Property(e => e.EmergencyPhone).HasMaxLength(20);
        builder.Property(e => e.GeneralNotes).HasColumnType("nvarchar(max)");
        builder.HasOne(d => d.Clinic)
            .WithMany(p => p.Patients)
            .HasForeignKey(d => d.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

