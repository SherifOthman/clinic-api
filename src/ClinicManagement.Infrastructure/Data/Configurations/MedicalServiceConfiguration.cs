using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class MedicalServiceConfiguration : IEntityTypeConfiguration<MedicalService>
{
    public void Configure(EntityTypeBuilder<MedicalService> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.DefaultPrice)
            .HasPrecision(18, 2);

        builder.HasOne(s => s.ClinicBranch)
            .WithMany(cb => cb.MedicalServices)
            .HasForeignKey(s => s.ClinicBranchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.ClinicBranchId, s.Name })
            .IsUnique();
    }
}
