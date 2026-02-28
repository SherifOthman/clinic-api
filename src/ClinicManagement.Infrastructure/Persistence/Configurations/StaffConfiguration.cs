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
        
        builder.HasOne(s => s.Clinic)
            .WithMany(c => c.Staff)
            .HasForeignKey(s => s.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(s => s.DoctorProfile)
            .WithOne(dp => dp.Staff)
            .HasForeignKey<DoctorProfile>(dp => dp.StaffId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
