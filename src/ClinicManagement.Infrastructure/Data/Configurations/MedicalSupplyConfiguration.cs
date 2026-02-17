using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class MedicalSupplyConfiguration : IEntityTypeConfiguration<MedicalSupply>
{
    public void Configure(EntityTypeBuilder<MedicalSupply> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.UnitPrice)
            .HasPrecision(18, 2);

        builder.HasOne(s => s.ClinicBranch)
            .WithMany(cb => cb.MedicalSupplies)
            .HasForeignKey(s => s.ClinicBranchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.ClinicBranchId, s.Name })
            .IsUnique();
    }
}
