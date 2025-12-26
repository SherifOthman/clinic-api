using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class ReceptionistConfiguration : IEntityTypeConfiguration<Receptionist>
{
    public void Configure(EntityTypeBuilder<Receptionist> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasOne(r => r.User)
            .WithOne(u => u.Receptionist)
            .HasForeignKey<Receptionist>(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Clinic)
            .WithMany(c => c.Receptionists)
            .HasForeignKey(r => r.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Appointments)
            .WithOne(a => a.Receptionist)
            .HasForeignKey(a => a.ReceptionistId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(r => r.UserId).IsUnique();
        builder.HasIndex(r => r.ClinicId);
    }
}
