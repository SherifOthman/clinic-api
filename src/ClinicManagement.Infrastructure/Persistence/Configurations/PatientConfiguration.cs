using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Persistence.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.Property(p => p.PatientCode).HasMaxLength(50).IsRequired();
        builder.Property(p => p.FullName).HasMaxLength(200).IsRequired();
        builder.Property(p => p.EmergencyContactName).HasMaxLength(200);
        builder.Property(p => p.EmergencyContactPhone).HasMaxLength(20);
        builder.Property(p => p.EmergencyContactRelation).HasMaxLength(50);

        // No nav property to Clinic — tenant FK only
        builder.HasOne<Clinic>()
            .WithMany()
            .HasForeignKey(p => p.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Phones)
            .WithOne()
            .HasForeignKey(pp => pp.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Allergies)
            .WithOne()
            .HasForeignKey(pa => pa.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.ChronicDiseases)
            .WithOne()
            .HasForeignKey(pcd => pcd.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
