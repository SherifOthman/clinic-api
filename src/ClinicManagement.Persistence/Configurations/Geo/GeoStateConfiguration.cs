using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class GeoStateConfiguration : IEntityTypeConfiguration<GeoState>
{
    public void Configure(EntityTypeBuilder<GeoState> builder)
    {
        builder.ToTable("GeoStates");
        builder.HasKey(s => s.GeonameId);
        builder.Property(s => s.GeonameId).ValueGeneratedNever();
        builder.Property(s => s.NameEn).HasMaxLength(150).IsRequired();
        builder.Property(s => s.NameAr).HasMaxLength(150).IsRequired();
        builder.HasIndex(s => s.CountryGeonameId);

        builder.HasOne(s => s.Country)
               .WithMany(c => c.States)
               .HasForeignKey(s => s.CountryGeonameId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
