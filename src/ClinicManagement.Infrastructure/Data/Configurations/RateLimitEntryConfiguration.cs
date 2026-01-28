using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class RateLimitEntryConfiguration : IEntityTypeConfiguration<RateLimitEntry>
{
    public void Configure(EntityTypeBuilder<RateLimitEntry> builder)
    {
        builder.ToTable("RateLimitEntries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Identifier)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.RequestCount)
            .IsRequired();

        builder.Property(x => x.WindowStart)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        // Indexes for performance
        builder.HasIndex(x => new { x.Identifier, x.Type });
        builder.HasIndex(x => x.ExpiresAt);
    }
}