using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class BranchVisitTypeConfiguration : IEntityTypeConfiguration<BranchVisitType>
{
    public void Configure(EntityTypeBuilder<BranchVisitType> builder)
    {
        // Composite primary key
        builder.HasKey(bvt => new { bvt.ClinicBranchId, bvt.VisitTypeId });

        builder.Property(bvt => bvt.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.HasOne(bvt => bvt.Branch)
            .WithMany(b => b.BranchVisitTypes)
            .HasForeignKey(bvt => bvt.ClinicBranchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(bvt => bvt.VisitType)
            .WithMany(vt => vt.BranchVisitTypes)
            .HasForeignKey(bvt => bvt.VisitTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
