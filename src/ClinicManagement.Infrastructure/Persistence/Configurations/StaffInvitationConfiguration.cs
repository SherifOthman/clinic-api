using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Persistence.Configurations;

public class StaffInvitationConfiguration : IEntityTypeConfiguration<StaffInvitation>
{
    public void Configure(EntityTypeBuilder<StaffInvitation> builder)
    {
        builder.Property(si => si.Email).HasMaxLength(256).IsRequired();
        builder.Property(si => si.Role).HasMaxLength(50).IsRequired();
        builder.Property(si => si.InvitationToken).HasMaxLength(100).IsRequired();
        
        builder.HasIndex(si => si.InvitationToken).IsUnique();
        
        builder.HasOne(si => si.Clinic)
            .WithMany()
            .HasForeignKey(si => si.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(si => si.Specialization)
            .WithMany()
            .HasForeignKey(si => si.SpecializationId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(si => si.AcceptedByUser)
            .WithMany()
            .HasForeignKey(si => si.AcceptedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(si => si.CreatedByUser)
            .WithMany()
            .HasForeignKey(si => si.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
