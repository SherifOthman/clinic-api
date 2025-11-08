using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class ClinicSettingsConfiguration : IEntityTypeConfiguration<ClinicSettings>
{
    public void Configure(EntityTypeBuilder<ClinicSettings> builder)
    {
        builder.ToTable("ClinicSettings");
        builder.Property(e => e.FasterPrice).HasColumnType("decimal(10,2)");
        builder.Property(e => e.CheckUpPrice).HasColumnType("decimal(10,2)");
        builder.Property(e => e.RevisitPrice).HasColumnType("decimal(10,2)");
        builder.Property(e => e.Currency).HasMaxLength(10);
        builder.HasOne(d => d.Clinic)
            .WithMany(p => p.Settings)
            .HasForeignKey(d => d.ClinicId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

