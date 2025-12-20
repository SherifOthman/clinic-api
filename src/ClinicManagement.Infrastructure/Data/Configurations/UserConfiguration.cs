using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
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

        // Relationships
        builder.HasOne(u => u.Clinic)
            .WithMany()
            .HasForeignKey(u => u.ClinicId)
            .OnDelete(DeleteBehavior.NoAction); // Prevent cascade delete cycles

        // Indexes
        builder.HasIndex(u => u.Email);
        builder.HasIndex(u => u.UserName);
        builder.HasIndex(u => u.ClinicId);

        // Seed SystemAdmin user
        // Password: Admin@123 (pre-hashed to avoid non-deterministic values)
        builder.HasData(new User
        {
            Id = 1,
            UserName = "sysadmin",
            NormalizedUserName = "SYSADMIN",
            Email = "admin@test.com",
            NormalizedEmail = "ADMIN@TEST.COM",
            EmailConfirmed = true,
            FirstName = "System",
            LastName = "Administrator",
            Avatar = "https://i.pravatar.cc/150?u=admin",
            PhoneNumber = "+1 (555) 999-0000",
            PhoneNumberConfirmed = true,
            ClinicId = null,
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            SecurityStamp = "f2287ff4-ca36-49d7-93fe-4ef70691ddf5",
            ConcurrencyStamp = "b010b791-c426-450b-b2c0-f4374cd73871",
            PasswordHash = "AQAAAAIAAYagAAAAEMCKDYlds+mz5+QQ23WfxO3uYIfe1xLh75u2hQ24rUBw8gyuADBeA5zEvAljasZv6g=="
        });
    }
}
