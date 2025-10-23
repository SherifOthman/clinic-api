using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class ClinicBranchConfiguration : IEntityTypeConfiguration<ClinicBranch>
{
    public void Configure(EntityTypeBuilder<ClinicBranch> builder)
    {
        builder.ToTable("ClinicBranches");
        builder.Property(e => e.City).HasMaxLength(50);
        builder.Property(e => e.Address).HasMaxLength(200);
        builder.Property(e => e.Phone).HasMaxLength(20);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
        builder.HasOne(d => d.Clinic)
            .WithMany(p => p.Branches)
            .HasForeignKey(d => d.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(d => d.CityNavigation)
            .WithMany(p => p.ClinicBranches)
            .HasForeignKey(d => d.CityId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

