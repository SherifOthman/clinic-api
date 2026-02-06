using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class LabTechnicianConfiguration : IEntityTypeConfiguration<LabTechnician>
{
    public void Configure(EntityTypeBuilder<LabTechnician> builder)
    {
        builder.ToTable("LabTechnicians");

        builder.HasKey(lt => lt.Id);

        builder.Property(lt => lt.UserId)
            .IsRequired();

        builder.Property(lt => lt.Certification)
            .HasMaxLength(200);

        builder.Property(lt => lt.Specialization)
            .HasMaxLength(100);

        // One-to-one relationship with User
        builder.HasOne(lt => lt.User)
            .WithOne(u => u.LabTechnician)
            .HasForeignKey<LabTechnician>(lt => lt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(lt => lt.UserId)
            .IsUnique();
    }
}
