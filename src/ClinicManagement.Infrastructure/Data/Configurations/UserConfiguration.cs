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

        // Relationships - User can own only one clinic
        builder.HasOne(u => u.OwnedClinic)
            .WithOne(c => c.Owner)
            .HasForeignKey<Clinic>(c => c.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Doctor)
            .WithOne(d => d.User)
            .HasForeignKey<Doctor>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.Receptionist)
            .WithOne(r => r.User)
            .HasForeignKey<Receptionist>(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(u => u.Email);
        builder.HasIndex(u => u.UserName);
    }
}
