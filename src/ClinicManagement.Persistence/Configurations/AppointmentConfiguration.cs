using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.Property(a => a.AppointmentNumber).HasMaxLength(50).IsRequired();

        builder.Property(a => a.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(a => a.Type)
            .HasConversion<short>()
            .HasColumnType("smallint");

        builder.Property(a => a.Status)
            .HasConversion<short>()
            .HasColumnType("smallint");

        // Branch FK — NoAction to avoid cascade cycles
        builder.HasOne(a => a.Branch)
            .WithMany(b => b.Appointment)
            .HasForeignKey(a => a.ClinicBranchId)
            .OnDelete(DeleteBehavior.NoAction);

        // Patient FK
        builder.HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.NoAction);

        // Doctor FK — NoAction to avoid cascade cycles
        builder.HasOne(a => a.Doctor)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.NoAction);

        // VisitType FK — price snapshot, restrict delete
        builder.HasOne(a => a.VisitType)
            .WithMany(vt => vt.Appointments)
            .HasForeignKey(a => a.VisitTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique: one doctor can't have two queue slots on the same date
        builder.HasIndex(a => new { a.DoctorId, a.Date, a.QueueNumber })
            .IsUnique()
            .HasFilter("[QueueNumber] IS NOT NULL");

        // Unique: one doctor can't have two time slots at the same time on the same date
        builder.HasIndex(a => new { a.DoctorId, a.Date, a.ScheduledTime })
            .IsUnique()
            .HasFilter("[ScheduledTime] IS NOT NULL");

        builder.HasIndex(a => new { a.ClinicBranchId, a.Date, a.Status });
        builder.HasIndex(a => new { a.PatientId, a.Date });
    }
}
