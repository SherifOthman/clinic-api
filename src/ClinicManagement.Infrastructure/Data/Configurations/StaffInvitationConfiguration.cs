using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class StaffInvitationConfiguration : IEntityTypeConfiguration<StaffInvitation>
{
    public void Configure(EntityTypeBuilder<StaffInvitation> builder)
    {
        builder.ToTable("StaffInvitations");

        builder.HasKey(si => si.Id);

        builder.Property(si => si.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(si => si.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(si => si.UserType)
            .IsRequired();

        builder.HasOne(si => si.Clinic)
            .WithMany()
            .HasForeignKey(si => si.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(si => si.InvitedByUser)
            .WithMany()
            .HasForeignKey(si => si.InvitedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(si => si.AcceptedByUser)
            .WithMany()
            .HasForeignKey(si => si.AcceptedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(si => si.Token).IsUnique();
        builder.HasIndex(si => new { si.Email, si.ClinicId, si.IsAccepted });
    }
}
