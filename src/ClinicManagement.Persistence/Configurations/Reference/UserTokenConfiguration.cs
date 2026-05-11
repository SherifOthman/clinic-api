using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class UserTokenConfiguration : IEntityTypeConfiguration<UserToken>
{
    public void Configure(EntityTypeBuilder<UserToken> builder)
    {
        builder.ToTable("OtpTokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TokenType).HasMaxLength(30).IsRequired();
        builder.Property(t => t.TokenHash).HasMaxLength(64).IsRequired(); // SHA-256 hex = 64 chars

        // Unique lookup: find active token by hash
        builder.HasIndex(t => t.TokenHash).IsUnique();

        // Cleanup query: find all tokens for a user+type to invalidate old ones
        builder.HasIndex(t => new { t.UserId, t.TokenType });

        // Cleanup job: delete expired tokens
        builder.HasIndex(t => t.ExpiresAt);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(t =>
            t.HasCheckConstraint("CK_OtpTokens_TokenType",
                "[TokenType] IN ('EmailConfirmation', 'PasswordReset')"));
    }
}
