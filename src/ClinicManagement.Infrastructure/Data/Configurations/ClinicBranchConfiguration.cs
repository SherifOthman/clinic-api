using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class ClinicBranchConfiguration : IEntityTypeConfiguration<ClinicBranch>
{
    public void Configure(EntityTypeBuilder<ClinicBranch> builder)
    {
        builder.ToTable("ClinicBranches");

        builder.HasKey(cb => cb.Id);

        builder.Property(cb => cb.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(cb => cb.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasOne(cb => cb.Clinic)
            .WithMany(c => c.Branches)
            .HasForeignKey(cb => cb.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);

        // Store only CityId - Country and State derived through joins
        builder.HasOne(cb => cb.City)
            .WithMany()
            .HasForeignKey(cb => cb.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index on CityId for efficient location-based queries
        builder.HasIndex(cb => cb.CityId);
    }
}
