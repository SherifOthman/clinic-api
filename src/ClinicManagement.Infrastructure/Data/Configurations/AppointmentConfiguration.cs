using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

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

        builder.Property(a => a.FinalPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(a => a.DiscountAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.Property(a => a.PaidAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        // Relationships
        builder.HasOne(a => a.ClinicBranch)
            .WithMany()
            .HasForeignKey(a => a.ClinicBranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.ClinicPatient)
            .WithMany()
            .HasForeignKey(a => a.ClinicPatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Doctor)
            .WithMany()
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.AppointmentType)
            .WithMany(at => at.Appointments)
            .HasForeignKey(a => a.AppointmentTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(a => a.AppointmentDate);
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.AppointmentTypeId);
        builder.HasIndex(a => new { a.ClinicBranchId, a.AppointmentDate });
        builder.HasIndex(a => new { a.DoctorId, a.AppointmentDate });
        builder.HasIndex(a => new { a.ClinicPatientId, a.AppointmentDate });
    }
}