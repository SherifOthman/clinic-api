using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");
        builder.Property(e => e.Price).HasColumnType("decimal(10,2)");
        builder.Property(e => e.PaidPrice).HasColumnType("decimal(10,2)");
        builder.Property(e => e.Discount).HasColumnType("decimal(10,2)");
        builder.Property(e => e.Notes).HasMaxLength(300);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");
        builder.HasOne(d => d.Branch)
            .WithMany(p => p.Appointments)
            .HasForeignKey(d => d.BranchId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(d => d.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(d => d.PatientId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(d => d.Doctor)
            .WithMany(p => p.Appointments)
            .HasForeignKey(d => d.DoctorId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(d => d.Receptionist)
            .WithMany(p => p.Appointments)
            .HasForeignKey(d => d.ReceptionistId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

