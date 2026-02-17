using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class MedicalVisitConfiguration : IEntityTypeConfiguration<MedicalVisit>
{
    public void Configure(EntityTypeBuilder<MedicalVisit> builder)
    {
        builder.ToTable("MedicalVisit");

        builder.HasKey(mv => mv.Id);

        // Relationships
        builder.HasOne(mv => mv.ClinicBranch)
            .WithMany()
            .HasForeignKey(mv => mv.ClinicBranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(mv => mv.Patient)
            .WithMany()
            .HasForeignKey(mv => mv.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(mv => mv.DoctorProfile)
            .WithMany(d => d.MedicalVisits)
            .HasForeignKey(mv => mv.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(mv => mv.Appointment)
            .WithMany()
            .HasForeignKey(mv => mv.AppointmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(mv => mv.PatientId);
        builder.HasIndex(mv => mv.DoctorId);
        builder.HasIndex(mv => mv.AppointmentId);
    }
}
