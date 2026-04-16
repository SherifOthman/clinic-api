using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class DoctorVisitTypeConfiguration : IEntityTypeConfiguration<DoctorVisitType>
{
    public void Configure(EntityTypeBuilder<DoctorVisitType> builder)
    {
        builder.Property(v => v.NameAr).HasMaxLength(100).IsRequired();
        builder.Property(v => v.NameEn).HasMaxLength(100).IsRequired();
        builder.Property(v => v.Price).HasColumnType("decimal(18,2)").IsRequired();

        builder.HasOne(v => v.Doctor)
            .WithMany(d => d.VisitTypes)
            .HasForeignKey(v => v.DoctorId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.Branch)
            .WithMany(b => b.DoctorVisitTypes)
            .HasForeignKey(v => v.ClinicBranchId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(v => new { v.DoctorId, v.ClinicBranchId, v.IsActive });
    }
}
