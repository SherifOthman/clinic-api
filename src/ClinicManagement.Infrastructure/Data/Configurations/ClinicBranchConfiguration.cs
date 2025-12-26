using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class ClinicBranchConfiguration : IEntityTypeConfiguration<ClinicBranch>
{
    public void Configure(EntityTypeBuilder<ClinicBranch> builder)
    {
        builder.HasKey(cb => cb.Id);

        builder.Property(cb => cb.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(cb => cb.Address)
            .HasMaxLength(500);

        builder.Property(cb => cb.PhoneNumber)
            .HasMaxLength(20);

        // Relationships
        builder.HasOne(cb => cb.Clinic)
            .WithMany(c => c.Branches)
            .HasForeignKey(cb => cb.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(cb => cb.Appointments)
            .WithOne(a => a.Branch)
            .HasForeignKey(a => a.BranchId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(cb => cb.ClinicId);
        builder.HasIndex(cb => cb.Name);
    }
}