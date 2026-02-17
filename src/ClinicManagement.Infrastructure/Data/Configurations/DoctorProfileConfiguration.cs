using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class DoctorProfileConfiguration : IEntityTypeConfiguration<DoctorProfile>
{
    public void Configure(EntityTypeBuilder<DoctorProfile> builder)
    {
        builder.ToTable("DoctorProfiles");
        
        builder.HasKey(d => d.Id);
        
        builder.Property(d => d.LicenseNumber)
            .HasMaxLength(50);
        
        // Relationships
        builder.HasOne(d => d.Staff)
            .WithOne(s => s.DoctorProfile)
            .HasForeignKey<DoctorProfile>(d => d.StaffId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(d => d.Specialization)
            .WithMany(s => s.DoctorProfiles)
            .HasForeignKey(d => d.SpecializationId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasMany(d => d.Appointments)
            .WithOne(a => a.DoctorProfile)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes
        builder.HasIndex(d => d.StaffId).IsUnique();
        builder.HasIndex(d => d.SpecializationId);
    }
}
