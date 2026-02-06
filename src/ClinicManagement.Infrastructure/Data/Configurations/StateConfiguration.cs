using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class StateConfiguration : IEntityTypeConfiguration<State>
{
    public void Configure(EntityTypeBuilder<State> builder)
    {
        builder.ToTable("States");

        builder.HasKey(s => s.Id);

        // GeoNames ID - unique and indexed for lazy seeding lookups
        builder.Property(s => s.GeonameId)
            .IsRequired();
        builder.HasIndex(s => s.GeonameId)
            .IsUnique();

        builder.Property(s => s.NameEn)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.NameAr)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(s => s.Country)
            .WithMany(c => c.States)
            .HasForeignKey(s => s.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index on CountryId for efficient joins
        builder.HasIndex(s => s.CountryId);
    }
}
