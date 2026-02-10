using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.ClinicId)
            .IsRequired(false); // Nullable for SuperAdmin

        builder.Property(u => u.UserType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(u => u.ProfileImageUrl)
            .HasMaxLength(500);

        // Relationship with Clinic (optional for SuperAdmin)
        builder.HasOne(u => u.Clinic)
            .WithMany()
            .HasForeignKey(u => u.ClinicId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Relationships with RefreshTokens
        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(u => u.Email);
        builder.HasIndex(u => u.UserName);
        builder.HasIndex(u => u.ClinicId);
        builder.HasIndex(u => u.UserType);
    }
}

