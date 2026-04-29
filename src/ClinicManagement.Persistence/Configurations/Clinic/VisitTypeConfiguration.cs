using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class VisitTypeConfiguration : IEntityTypeConfiguration<VisitType>
{
    public void Configure(EntityTypeBuilder<VisitType> builder)
    {
        builder.Property(v => v.Name).HasMaxLength(100).IsRequired();
        builder.Property(v => v.Price).HasColumnType("decimal(18,2)").IsRequired();

        builder.ToTable(t => t.HasCheckConstraint("CK_VisitType_Price", "[Price] >= 0"));

        builder.HasOne(v => v.Schedule)
            .WithMany(s => s.VisitTypes)
            .HasForeignKey(v => v.DoctorBranchScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(v => new { v.DoctorBranchScheduleId, v.IsActive });
    }
}
