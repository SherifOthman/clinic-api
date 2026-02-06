using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Countries");

        builder.HasKey(c => c.Id);

        // GeoNames ID - unique and indexed for lazy seeding lookups
        builder.Property(c => c.GeonameId)
            .IsRequired();
        builder.HasIndex(c => c.GeonameId)
            .IsUnique();

        builder.Property(c => c.Iso2Code)
            .IsRequired()
            .HasMaxLength(2);
        builder.HasIndex(c => c.Iso2Code)
            .IsUnique();

        builder.Property(c => c.PhoneCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(c => c.NameEn)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.NameAr)
            .IsRequired()
            .HasMaxLength(100);
    }
}
