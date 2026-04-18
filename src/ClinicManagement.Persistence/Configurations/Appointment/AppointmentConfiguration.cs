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
        builder.Property(a => a.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Appointment_Price",       "[Price] >= 0");
            t.HasCheckConstraint("CK_Appointment_FinalPrice",  "[FinalPrice] >= 0");
            t.HasCheckConstraint("CK_Appointment_Discount",    "[DiscountPercent] IS NULL OR ([DiscountPercent] >= 0 AND [DiscountPercent] <= 100)");
            t.HasCheckConstraint("CK_Appointment_QueueNumber", "[QueueNumber] IS NULL OR [QueueNumber] > 0");
            t.HasCheckConstraint("CK_Appointment_Type",        "[Type] IN ('Queue', 'Time')");
            t.HasCheckConstraint("CK_Appointment_Status",      "[Status] IN ('Pending', 'InProgress', 'Completed', 'Cancelled', 'NoShow')");
        });

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
