using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("Doctors");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.UserId)
            .IsRequired();

        builder.Property(d => d.SpecializationId)
            .IsRequired();

        builder.Property(d => d.LicenseNumber)
            .HasMaxLength(100);

        builder.Property(d => d.YearsOfExperience)
            .HasColumnType("smallint");

        builder.Property(d => d.ConsultationFee)
            .HasColumnType("decimal(18,2)");

        builder.Property(d => d.Biography)
            .HasMaxLength(2000);

        // One-to-one relationship with User
        builder.HasOne(d => d.User)
            .WithOne(u => u.Doctor)
            .HasForeignKey<Doctor>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Foreign key relationship with Specialization
        builder.HasOne(d => d.Specialization)
            .WithMany(s => s.Doctors)
            .HasForeignKey(d => d.SpecializationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for performance
        builder.HasIndex(d => d.UserId)
            .IsUnique();
        
        builder.HasIndex(d => d.SpecializationId);
        
        builder.HasIndex(d => d.LicenseNumber);
    }
}

