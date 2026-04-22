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

        // Index for FK lookups
        builder.HasIndex(c => c.StateGeonameId);

        // Note: no unique index on (StateGeonameId, NameEn) — GeoNames data legitimately
        // has cities with the same name in the same state (e.g. multiple villages named
        // "Roßbach" in Bavaria). The PK (GeonameId) is the true unique identifier.

        builder.HasOne(c => c.State)
               .WithMany(s => s.Cities)
               .HasForeignKey(c => c.StateGeonameId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
