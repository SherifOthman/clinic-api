using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Clinic>()
            .WithMany()
            .HasForeignKey(s => s.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);

        // DoctorProfile relationship is configured in DoctorProfileConfiguration
        // using HasOne(dp => dp.Staff).WithOne(s => s.DoctorProfile)

        builder.HasIndex(s => new { s.ClinicId, s.IsDeleted, s.IsActive });
        builder.HasIndex(s => s.UserId).IsUnique();
    }
}
