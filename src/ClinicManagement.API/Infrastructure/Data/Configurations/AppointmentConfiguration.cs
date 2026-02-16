using ClinicManagement.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.API.Infrastructure.Data.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");

        builder.HasKey(a => a.Id);

        // Properties
        builder.Property(a => a.AppointmentDate)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(a => a.QueueNumber)
            .IsRequired()
            .HasColumnType("smallint");

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<byte>();

        // Ignore calculated properties (not stored in database)
        builder.Ignore(a => a.IsConsultationFeePaid);
        builder.Ignore(a => a.IsPending);
        builder.Ignore(a => a.IsConfirmed);
        builder.Ignore(a => a.IsCompleted);
        builder.Ignore(a => a.IsCancelled);

        // Relationships
        builder.HasOne(a => a.ClinicBranch)
            .WithMany()
            .HasForeignKey(a => a.ClinicBranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Patient)
            .WithMany()
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.DoctorProfile)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.AppointmentType)
            .WithMany(at => at.Appointments)
            .HasForeignKey(a => a.AppointmentTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Invoice)
            .WithMany()
            .HasForeignKey(a => a.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(a => a.AppointmentDate);
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.AppointmentTypeId);
        builder.HasIndex(a => new { a.ClinicBranchId, a.AppointmentDate });
        builder.HasIndex(a => new { a.DoctorId, a.AppointmentDate });
        builder.HasIndex(a => new { a.PatientId, a.AppointmentDate });
    }
}
