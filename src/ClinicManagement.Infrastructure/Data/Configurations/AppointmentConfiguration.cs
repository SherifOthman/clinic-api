using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");
        
        builder.Property(e => e.Notes).HasMaxLength(500);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");
        
        builder.HasOne(d => d.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(d => d.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(d => d.Doctor)
            .WithMany(p => p.Appointments)
            .HasForeignKey(d => d.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(d => d.Receptionist)
            .WithMany(p => p.Appointments)
            .HasForeignKey(d => d.ReceptionistId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(d => d.Branch)
            .WithMany(b => b.Appointments)
            .HasForeignKey(d => d.BranchId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(a => a.PatientId);
        builder.HasIndex(a => a.DoctorId);
        builder.HasIndex(a => a.BranchId);
        builder.HasIndex(a => a.AppointmentDate);
        builder.HasIndex(a => a.Status);
    }
}

