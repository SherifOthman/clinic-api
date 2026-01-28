using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class ClinicBranchConfiguration : IEntityTypeConfiguration<ClinicBranch>
{
    public void Configure(EntityTypeBuilder<ClinicBranch> builder)
    {
        builder.ToTable("ClinicBranches");
        
        builder.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(e => e.Address)
            .HasMaxLength(500)
            .IsRequired();
        
        // GeoNames integration properties
        builder.Property(e => e.GeoNameId).IsRequired();
        builder.Property(e => e.CityName)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(e => e.StateName)
            .HasMaxLength(100);
        builder.Property(e => e.CountryCode).HasMaxLength(2).IsRequired();
        builder.Property(e => e.Latitude).HasPrecision(10, 7).IsRequired();
        builder.Property(e => e.Longitude).HasPrecision(10, 7).IsRequired();

        // Relationships
        builder.HasOne(cb => cb.Clinic)
            .WithMany(c => c.ClinicBranches)
            .HasForeignKey(cb => cb.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(cb => cb.PhoneNumbers)
            .WithOne(pn => pn.ClinicBranch)
            .HasForeignKey(pn => pn.ClinicBranchId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(cb => cb.ClinicId);
        builder.HasIndex(cb => cb.GeoNameId);
        builder.HasIndex(cb => cb.CountryCode);
    }
}