using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class PharmacistConfiguration : IEntityTypeConfiguration<Pharmacist>
{
    public void Configure(EntityTypeBuilder<Pharmacist> builder)
    {
        builder.ToTable("Pharmacists");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(p => p.LicenseNumber)
            .HasMaxLength(100);

        builder.Property(p => p.Specialization)
            .HasMaxLength(100);

        // One-to-one relationship with User
        builder.HasOne(p => p.User)
            .WithOne(u => u.Pharmacist)
            .HasForeignKey<Pharmacist>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(p => p.UserId)
            .IsUnique();
        
        builder.HasIndex(p => p.LicenseNumber);
    }
}
