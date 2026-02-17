using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder.ToTable("Staff");
        
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.Property(s => s.HireDate)
            .IsRequired();
        
        // Relationships
        builder.HasOne(s => s.User)
            .WithMany(u => u.StaffMemberships)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(s => s.Clinic)
            .WithMany(c => c.Staff)
            .HasForeignKey(s => s.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(s => s.DoctorProfile)
            .WithOne(d => d.Staff)
            .HasForeignKey<DoctorProfile>(d => d.StaffId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes
        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.ClinicId);
        builder.HasIndex(s => new { s.UserId, s.ClinicId }).IsUnique();
    }
}
