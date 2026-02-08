using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class LocationSnapshotConfiguration : IEntityTypeConfiguration<LocationSnapshot>
{
    public void Configure(EntityTypeBuilder<LocationSnapshot> builder)
    {
        builder.ToTable("LocationSnapshots");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.GeoNameId)
            .IsRequired();

        builder.Property(l => l.Type)
            .IsRequired();

        builder.Property(l => l.NameEn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.NameAr)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.Provider)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("GeoNames");

        builder.Property(l => l.LastSyncedAt)
            .IsRequired();

        builder.HasIndex(l => l.GeoNameId)
            .IsUnique();

        builder.HasIndex(l => l.Type);
    }
}
