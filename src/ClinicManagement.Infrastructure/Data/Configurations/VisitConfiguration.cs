using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class VisitConfiguration : IEntityTypeConfiguration<Visit>
{
    public void Configure(EntityTypeBuilder<Visit> builder)
    {
        builder.ToTable("Visits");
        builder.Property(e => e.Description).HasColumnType("nvarchar(max)");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
        builder.HasOne(d => d.Appointment)
            .WithMany(p => p.Visits)
            .HasForeignKey(d => d.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

