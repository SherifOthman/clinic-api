using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class GeoCountryConfiguration : IEntityTypeConfiguration<GeoCountry>
{
    public void Configure(EntityTypeBuilder<GeoCountry> builder)
    {
        builder.ToTable("GeoCountries");
        builder.HasKey(c => c.GeonameId);
        builder.Property(c => c.GeonameId).ValueGeneratedNever();
        builder.Property(c => c.CountryCode).HasMaxLength(2).IsRequired();
        builder.Property(c => c.NameEn).HasMaxLength(100).IsRequired();
        builder.Property(c => c.NameAr).HasMaxLength(100).IsRequired();
        builder.HasIndex(c => c.CountryCode).IsUnique();
    }
}
