using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.ToTable("Cities");

        builder.HasKey(c => c.Id);

        // GeoNames ID - unique and indexed for lazy seeding lookups
        builder.Property(c => c.GeonameId)
            .IsRequired();
        builder.HasIndex(c => c.GeonameId)
            .IsUnique();

        builder.Property(c => c.NameEn)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.NameAr)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(c => c.State)
            .WithMany(s => s.Cities)
            .HasForeignKey(c => c.StateId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index on StateId for efficient joins
        builder.HasIndex(c => c.StateId);
        
        // Composite index for searching cities by state and name
        builder.HasIndex(c => new { c.StateId, c.NameEn });
    }
}
