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

        builder.Property(cb => cb.AddressLine)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(cb => cb.CountryGeoNameId)
            .IsRequired();

        builder.Property(cb => cb.StateGeoNameId)
            .IsRequired();

        builder.Property(cb => cb.CityGeoNameId)
            .IsRequired();

        builder.HasOne(cb => cb.Clinic)
            .WithMany(c => c.Branches)
            .HasForeignKey(cb => cb.ClinicId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(cb => cb.CityGeoNameId);
        builder.HasIndex(cb => cb.StateGeoNameId);
        builder.HasIndex(cb => cb.CountryGeoNameId);
    }
}
