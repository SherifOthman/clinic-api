using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class ClinicBranchAppointmentPriceConfiguration : IEntityTypeConfiguration<ClinicBranchAppointmentPrice>
{
    public void Configure(EntityTypeBuilder<ClinicBranchAppointmentPrice> builder)
    {
        builder.ToTable("ClinicBranchAppointmentPrices");

        builder.HasKey(cbap => cbap.Id);

        // Properties
        builder.Property(cbap => cbap.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(cbap => cbap.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(cbap => cbap.Description)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(cbap => cbap.ClinicBranch)
            .WithMany(cb => cb.AppointmentPrices)
            .HasForeignKey(cbap => cbap.ClinicBranchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cbap => cbap.AppointmentType)
            .WithMany(at => at.ClinicBranchPrices)
            .HasForeignKey(cbap => cbap.AppointmentTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(cbap => new { cbap.ClinicBranchId, cbap.AppointmentTypeId })
            .IsUnique()
            .HasDatabaseName("IX_ClinicBranchAppointmentPrices_ClinicBranchId_AppointmentTypeId");

        builder.HasIndex(cbap => cbap.ClinicBranchId);
        builder.HasIndex(cbap => cbap.AppointmentTypeId);
        builder.HasIndex(cbap => cbap.IsActive);
    }
}
