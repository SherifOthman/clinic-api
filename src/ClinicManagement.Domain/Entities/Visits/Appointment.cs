using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

public class Appointment : AuditableEntity, INoAuditLog
{
    public Guid BranchId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorInfoId { get; set; }
    public Guid VisitTypeId { get; set; }

    public DateOnly Date { get; set; }
    public int? QueueNumber { get; set; }
    public TimeOnly? ScheduledTime { get; set; }

    public AppointmentType Type { get; set; } = AppointmentType.Queue;
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

    public decimal Price { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal FinalPrice { get; set; }

    public Guid? InvoiceId { get; set; }

    // ── Computed ──────────────────────────────────────────────────────────────

    public bool IsPending    => Status == AppointmentStatus.Pending;
    public bool IsInProgress => Status == AppointmentStatus.InProgress;
    public bool IsCompleted  => Status == AppointmentStatus.Completed;
    public bool IsCancelled  => Status == AppointmentStatus.Cancelled;
    public bool IsNoShow     => Status == AppointmentStatus.NoShow;

    public bool IsQueued    => Type == AppointmentType.Queue;
    public bool IsScheduled => Type == AppointmentType.Time;

    public bool CanBeCancelled => Status == AppointmentStatus.Pending || Status == AppointmentStatus.InProgress;
    public bool IsInvoiced     => InvoiceId.HasValue;

    /// <summary>Applies DiscountPercent to Price and returns the final amount.</summary>
    public decimal CalculateFinalPrice() =>
        DiscountPercent.HasValue
            ? Math.Round(Price * (1 - DiscountPercent.Value / 100m), 2)
            : Price;

    // Navigation
    public ClinicBranch Branch { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public DoctorInfo Doctor { get; set; } = null!;
    public VisitType VisitType { get; set; } = null!;
}
