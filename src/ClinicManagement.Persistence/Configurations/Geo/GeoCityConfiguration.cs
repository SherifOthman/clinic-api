using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class GeoCityConfiguration : IEntityTypeConfiguration<GeoCity>
{
    public void Configure(EntityTypeBuilder<GeoCity> builder)
    {
        builder.ToTable("GeoCities");
        builder.HasKey(c => c.GeonameId);
        builder.Property(c => c.GeonameId).ValueGeneratedNever();
        builder.Property(c => c.NameEn).HasMaxLength(150).IsRequired();
        builder.Property(c => c.NameAr).HasMaxLength(150).IsRequired();
        builder.HasIndex(c => c.StateGeonameId);

        builder.HasOne(c => c.State)
               .WithMany(s => s.Cities)
               .HasForeignKey(c => c.StateGeonameId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
