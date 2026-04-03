using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Persistence.Configurations;

public class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Clinic is the tenant owner — no nav property on Staff side
        builder.HasOne<Clinic>()
            .WithMany()
            .HasForeignKey(s => s.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);

        // DoctorProfile back-reference removed — configure via DoctorProfile side only
        builder.HasOne(s => s.DoctorProfile)
            .WithOne()
            .HasForeignKey<DoctorProfile>(dp => dp.StaffId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
