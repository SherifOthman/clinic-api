using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class NurseConfiguration : IEntityTypeConfiguration<Nurse>
{
    public void Configure(EntityTypeBuilder<Nurse> builder)
    {
        builder.ToTable("Nurses");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.UserId)
            .IsRequired();

        builder.Property(n => n.Certification)
            .HasMaxLength(200);

        builder.Property(n => n.Department)
            .HasMaxLength(100);

        // One-to-one relationship with User
        builder.HasOne(n => n.User)
            .WithOne(u => u.Nurse)
            .HasForeignKey<Nurse>(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(n => n.UserId)
            .IsUnique();
    }
}
