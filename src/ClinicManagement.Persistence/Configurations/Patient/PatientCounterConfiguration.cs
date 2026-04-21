using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class PatientCounterConfiguration : IEntityTypeConfiguration<PatientCounter>
{
    public void Configure(EntityTypeBuilder<PatientCounter> builder)
    {
        builder.ToTable("PatientCounters");

        // PK is ClinicId — one row per clinic
        builder.HasKey(pc => pc.ClinicId);

        builder.Property(pc => pc.ClinicId).IsRequired();
        builder.Property(pc => pc.LastValue).IsRequired().HasDefaultValue(0);

        builder.HasOne<Clinic>()
            .WithOne()
            .HasForeignKey<PatientCounter>(pc => pc.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
