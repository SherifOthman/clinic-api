using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class MeasurementAttributeConfiguration : IEntityTypeConfiguration<MeasurementAttribute>
{
    public void Configure(EntityTypeBuilder<MeasurementAttribute> builder)
    {
        builder.HasKey(ma => ma.Id);

        builder.Property(ma => ma.NameEn)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ma => ma.NameAr)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ma => ma.DescriptionEn)
            .HasMaxLength(500);

        builder.Property(ma => ma.DescriptionAr)
            .HasMaxLength(500);

        builder.HasIndex(ma => ma.NameEn);
        builder.HasIndex(ma => ma.NameAr);

        builder.HasMany(ma => ma.VisitMeasurements)
            .WithOne(vm => vm.MeasurementAttribute)
            .HasForeignKey(vm => vm.MeasurementAttributeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(ma => ma.DoctorMeasurements)
            .WithOne(dm => dm.MeasurementAttribute)
            .HasForeignKey(dm => dm.MeasurementAttributeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(ma => ma.SpecializationDefaults)
            .WithOne(sd => sd.MeasurementAttribute)
            .HasForeignKey(sd => sd.MeasurementAttributeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
