using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

public class Appointment : AuditableEntity, INoAuditLog
{
    public string AppointmentNumber { get; set; } = null!;
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

    /// <summary>Visit type (كشف / إعادة) — FK to VisitType.</summary>
    public Guid VisitTypeId { get; set; }

    /// <summary>
    /// Price snapshot — copied from BranchVisitType.Price at booking time.
    /// Never recalculated from current pricing to preserve history.
    /// </summary>
    public decimal Price { get; set; }

    public Guid? InvoiceId { get; set; }

    // Navigation
    public ClinicBranch Branch { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public VisitType VisitType { get; set; } = null!;
}
