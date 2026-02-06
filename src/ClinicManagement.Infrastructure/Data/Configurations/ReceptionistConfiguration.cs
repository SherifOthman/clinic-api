using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class ReceptionistConfiguration : IEntityTypeConfiguration<Receptionist>
{
    public void Configure(EntityTypeBuilder<Receptionist> builder)
    {
        builder.ToTable("Receptionists");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.UserId)
            .IsRequired();

        builder.Property(r => r.ShiftPreference)
            .HasMaxLength(50);

        builder.Property(r => r.Languages)
            .HasMaxLength(200);

        // One-to-one relationship with User
        builder.HasOne(r => r.User)
            .WithOne(u => u.Receptionist)
            .HasForeignKey<Receptionist>(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(r => r.UserId)
            .IsUnique();
    }
}
