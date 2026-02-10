using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.ExpiryTime)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedByIp)
            .HasMaxLength(45);

        builder.Property(x => x.RevokedByIp)
            .HasMaxLength(45);

        builder.Property(x => x.ReplacedByToken)
            .HasMaxLength(500);

        // Foreign key relationship with User
        builder.HasOne(x => x.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for performance
        builder.HasIndex(x => x.Token).IsUnique();
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ExpiryTime);
        builder.HasIndex(x => new { x.UserId, x.IsRevoked });
    }
}
