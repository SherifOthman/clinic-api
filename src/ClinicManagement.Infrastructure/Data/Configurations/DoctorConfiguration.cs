using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("Doctors");
        builder.Property(e => e.Bio).HasMaxLength(300);
        builder.HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId);
        builder.HasOne(d => d.Specialization)
            .WithMany(p => p.Doctors)
            .HasForeignKey(d => d.SpecializationId);
    }
}

