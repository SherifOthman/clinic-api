using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class QueueCounterConfiguration : IEntityTypeConfiguration<QueueCounter>
{
    public void Configure(EntityTypeBuilder<QueueCounter> builder)
    {
        builder.ToTable("QueueCounters");
        builder.HasKey(q => new { q.DoctorInfoId, q.Date });

        builder.Property(q => q.Date)
            .HasColumnType("date");

        builder.HasOne<DoctorInfo>()
            .WithMany()
            .HasForeignKey(q => q.DoctorInfoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
