using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class MedicineConfiguration : BaseEntityConfiguration<Medicine>
{
    public override void Configure(EntityTypeBuilder<Medicine> builder)
    {
        base.Configure(builder);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.BoxPrice)
            .HasPrecision(18, 2);

        builder.HasOne(m => m.ClinicBranch)
            .WithMany(cb => cb.Medicines)
            .HasForeignKey(m => m.ClinicBranchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => new { m.ClinicBranchId, m.Name })
            .IsUnique();
    }
}
