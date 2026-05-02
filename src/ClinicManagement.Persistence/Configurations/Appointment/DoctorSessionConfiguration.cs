using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class DoctorSessionConfiguration : IEntityTypeConfiguration<DoctorSession>
{
    public void Configure(EntityTypeBuilder<DoctorSession> builder)
    {
        builder.ToTable("DoctorSessions");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Date).HasColumnType("date");
        builder.Property(s => s.DelayHandling).HasConversion<string>().HasMaxLength(20);

        builder.ToTable("DoctorSessions", t => t.HasCheckConstraint(
            "CK_DoctorSession_DelayHandling",
            "[DelayHandling] IS NULL OR [DelayHandling] IN ('AutoShift', 'MarkMissed', 'Manual')"));

        builder.HasIndex(s => new { s.DoctorInfoId, s.BranchId, s.Date }).IsUnique();

        builder.HasOne(s => s.Doctor)
            .WithMany()
            .HasForeignKey(s => s.DoctorInfoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Branch)
            .WithMany()
            .HasForeignKey(s => s.BranchId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
