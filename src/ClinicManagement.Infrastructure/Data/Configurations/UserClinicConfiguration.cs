using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class UserClinicConfiguration : IEntityTypeConfiguration<UserClinic>
{
    public void Configure(EntityTypeBuilder<UserClinic> builder)
    {
        builder.ToTable("UserClinics");

        builder.HasKey(uc => uc.Id);

        builder.Property(uc => uc.UserId)
            .IsRequired();

        builder.Property(uc => uc.ClinicId)
            .IsRequired();

        builder.Property(uc => uc.IsOwner)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(uc => uc.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(uc => uc.JoinedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(uc => uc.User)
            .WithMany(u => u.UserClinics)
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(uc => uc.Clinic)
            .WithMany(c => c.UserClinics)
            .HasForeignKey(uc => uc.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(uc => new { uc.UserId, uc.ClinicId })
            .IsUnique()
            .HasDatabaseName("IX_UserClinics_UserId_ClinicId");

        builder.HasIndex(uc => uc.UserId)
            .HasDatabaseName("IX_UserClinics_UserId");

        builder.HasIndex(uc => uc.ClinicId)
            .HasDatabaseName("IX_UserClinics_ClinicId");
    }
}