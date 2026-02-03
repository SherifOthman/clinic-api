using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class MedicalServiceConfiguration : IEntityTypeConfiguration<MedicalService>
{
    public void Configure(EntityTypeBuilder<MedicalService> builder)
    {
        builder.ToTable("MedicalServices");
        
        // Primary key
        builder.HasKey(ms => ms.Id);
        
        // Properties
        builder.Property(ms => ms.ClinicId).IsRequired();
        builder.Property(ms => ms.Name)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(ms => ms.Description)
            .HasMaxLength(1000)
            .IsRequired(false);
        builder.Property(ms => ms.Price)
            .HasPrecision(18, 2)
            .IsRequired();
        builder.Property(ms => ms.Type).IsRequired();
        builder.Property(ms => ms.IsActive).IsRequired();
        
        // Relationships
        builder.HasOne(ms => ms.Clinic)
            .WithMany(c => c.Services)
            .HasForeignKey(ms => ms.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Collections
        builder.HasMany(ms => ms.TransactionServices)
            .WithOne(ts => ts.Service)
            .HasForeignKey(ts => ts.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes
        builder.HasIndex(ms => ms.ClinicId);
        builder.HasIndex(ms => ms.Name);
        builder.HasIndex(ms => ms.Type);
        builder.HasIndex(ms => ms.IsActive);
    }
}