using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.Property(a => a.Price).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(a => a.DiscountPercent).HasColumnType("decimal(5,2)");
        builder.Property(a => a.FinalPrice).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(a => a.Type).HasConversion<short>().HasColumnType("smallint");
        builder.Property(a => a.Status).HasConversion<short>().HasColumnType("smallint");

        builder.HasOne(a => a.Branch)
            .WithMany(b => b.Appointment)
            .HasForeignKey(a => a.BranchId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(a => a.Doctor)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DoctorInfoId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(a => a.VisitType)
            .WithMany(v => v.Appointments)
            .HasForeignKey(a => a.VisitTypeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.DoctorInfoId, a.Date, a.QueueNumber })
            .IsUnique()
            .HasFilter("[QueueNumber] IS NOT NULL");

        builder.HasIndex(a => new { a.DoctorInfoId, a.Date, a.ScheduledTime })
            .IsUnique()
            .HasFilter("[ScheduledTime] IS NOT NULL");

        builder.HasIndex(a => new { a.BranchId, a.Date, a.Status });
        builder.HasIndex(a => new { a.PatientId, a.Date });
    }
}
