using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class PatientSurgeryConfiguration : IEntityTypeConfiguration<PatientSurgery>
{
    public void Configure(EntityTypeBuilder<PatientSurgery> builder)
    {
        builder.ToTable("PatientSurgeries");
        builder.Property(e => e.Name).HasMaxLength(50);
        builder.Property(e => e.Description).HasColumnType("nvarchar(max)");
        builder.HasOne(d => d.Patient)
            .WithMany(p => p.Surgeries)
            .HasForeignKey(d => d.PatientId);
    }
}

