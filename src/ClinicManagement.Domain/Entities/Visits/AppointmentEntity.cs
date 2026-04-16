using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

public class Appointment : AuditableEntity, INoAuditLog
{
    public Guid ClinicBranchId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }

    public DateOnly Date { get; set; }

    /// <summary>Queue-based appointment number. Null for time-based appointments.</summary>
    public int? QueueNumber { get; set; }

    /// <summary>Scheduled time for time-based appointments. Null for queue-based.</summary>
    public TimeOnly? ScheduledTime { get; set; }

    public AppointmentType Type { get; set; } = AppointmentType.Queue;
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

    /// <summary>Visit type FK — links to DoctorVisitType.</summary>
    public Guid DoctorVisitTypeId { get; set; }

    /// <summary>Price snapshot — copied from DoctorVisitType.Price at booking time.</summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Discount percentage (0-100). Applied at booking time.
    /// FinalPrice = Price * (1 - DiscountPercent / 100).
    /// </summary>
    public decimal? DiscountPercent { get; set; }

    /// <summary>Final price after discount. Computed and stored at booking time.</summary>
    public decimal FinalPrice { get; set; }

    public Guid? InvoiceId { get; set; }

    // Navigation
    public ClinicBranch Branch { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public DoctorVisitType DoctorVisitType { get; set; } = null!;
}
