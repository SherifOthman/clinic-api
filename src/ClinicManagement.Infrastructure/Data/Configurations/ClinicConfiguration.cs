using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class ClinicConfiguration : IEntityTypeConfiguration<Clinic>
{
    public void Configure(EntityTypeBuilder<Clinic> builder)
    {
        builder.ToTable("Clinics");
        
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.PhoneNumber).HasMaxLength(20);
        builder.Property(e => e.Email).HasMaxLength(256);
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.Logo).HasMaxLength(500);
        builder.Property(e => e.Website).HasMaxLength(500);
        builder.Property(e => e.OnboardingStep).HasMaxLength(50);
        
        builder.HasOne(c => c.Owner)
            .WithMany()
            .HasForeignKey(c => c.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasIndex(c => c.OwnerId);
    }
}
