using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Inherits AuditableTenantEntity so the global tenant query filter applies automatically.
/// </summary>
public class Appointment : AuditableTenantEntity, IAuditableEntity, ISoftDeletable
{
    public Guid BranchId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorInfoId { get; set; }
    public Guid VisitTypeId { get; set; }

    public DateOnly Date { get; set; }
    public int? QueueNumber { get; set; }
    public TimeOnly? ScheduledTime { get; set; }
    /// <summary>Calculated end time = ScheduledTime + visit duration. Null for queue appointments.</summary>
    public TimeOnly? EndTime { get; set; }
    /// <summary>Per-appointment duration override in minutes. Null = use doctor's default.</summary>
    public int? VisitDurationMinutes { get; set; }

    public AppointmentType Type { get; set; } = AppointmentType.Queue;
    /// <summary>
    /// Status transitions are enforced by Transition() — never set directly.
    /// Private set prevents handlers from bypassing the domain rule.
    /// </summary>
    public AppointmentStatus Status { get; private set; } = AppointmentStatus.Pending;

    public decimal Price { get; set; }
    public decimal? DiscountPercent { get; set; }

    /// <summary>
    /// Stored final price — always set via ApplyPrice() to stay in sync with Price and DiscountPercent.
    /// Never set directly; use ApplyPrice() when creating or updating pricing.
    /// </summary>
    public decimal FinalPrice { get; private set; }

    public Guid? InvoiceId { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Navigation
    public ClinicBranch Branch { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public DoctorInfo Doctor { get; set; } = null!;
    public VisitType VisitType { get; set; } = null!;

    // ── Domain factory ────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new appointment with all required fields set consistently.
    /// QueueNumber must be assigned separately after creation (requires async counter).
    /// </summary>
    public static Appointment Create(
        Guid clinicId,
        Guid branchId,
        Guid patientId,
        Guid doctorInfoId,
        Guid visitTypeId,
        DateOnly date,
        AppointmentType type,
        TimeOnly? scheduledTime,
        int? visitDurationMinutes,
        decimal price,
        decimal? discountPercent = null)
    {
        var appointment = new Appointment
        {
            ClinicId             = clinicId,
            BranchId             = branchId,
            PatientId            = patientId,
            DoctorInfoId         = doctorInfoId,
            VisitTypeId          = visitTypeId,
            Date                 = date,
            Type                 = type,
            ScheduledTime        = type == AppointmentType.Time ? scheduledTime : null,
            VisitDurationMinutes = visitDurationMinutes,
            Status               = AppointmentStatus.Pending,
        };

        // Calculate end time for time-based appointments
        if (type == AppointmentType.Time && scheduledTime.HasValue)
        {
            var duration = visitDurationMinutes ?? 30;
            appointment.EndTime = scheduledTime.Value.AddMinutes(duration);
        }

        appointment.ApplyPrice(price, discountPercent);
        return appointment;
    }

    // ── Domain behaviour ──────────────────────────────────────────────────────

    /// <summary>
    /// Validates and applies a status transition.
    /// All allowed transitions are defined here — the handler just calls this and
    /// checks the result. Business rules stay in the domain, not in application code.
    /// </summary>
    public Result Transition(AppointmentStatus newStatus)
    {
        var allowed = (Status, newStatus) switch
        {
            // Normal forward flow
            (AppointmentStatus.Pending,    AppointmentStatus.Waiting)    => true,  // patient arrived
            (AppointmentStatus.Pending,    AppointmentStatus.InProgress) => true,  // direct (queue)
            (AppointmentStatus.Pending,    AppointmentStatus.Cancelled)  => true,
            (AppointmentStatus.Pending,    AppointmentStatus.NoShow)     => true,
            (AppointmentStatus.Waiting,    AppointmentStatus.InProgress) => true,  // called in
            (AppointmentStatus.Waiting,    AppointmentStatus.Cancelled)  => true,
            (AppointmentStatus.Waiting,    AppointmentStatus.NoShow)     => true,
            (AppointmentStatus.InProgress, AppointmentStatus.Completed)  => true,
            (AppointmentStatus.InProgress, AppointmentStatus.Cancelled)  => true,

            // Recovery paths — patient arrived late or receptionist made a mistake
            (AppointmentStatus.NoShow,    AppointmentStatus.Pending)     => true,  // patient showed up late
            (AppointmentStatus.NoShow,    AppointmentStatus.Waiting)     => true,  // arrived, skip to waiting
            (AppointmentStatus.Cancelled, AppointmentStatus.Pending)     => true,  // patient called back

            _ => false,
        };

        if (!allowed)
            return Result.Failure(
                ErrorCodes.OPERATION_NOT_ALLOWED,
                $"Cannot transition from {Status} to {newStatus}");

        Status = newStatus;
        return Result.Success();
    }

    public void ApplyPrice(decimal price, decimal? discountPercent = null)
    {
        Price           = price;
        DiscountPercent = discountPercent;
        FinalPrice      = discountPercent.HasValue
            ? Math.Round(price * (1 - discountPercent.Value / 100m), 2)
            : price;
    }

    /// <summary>
    /// Force-cancels the appointment regardless of current status.
    /// Used for bulk operations (doctor absent, session ended) where
    /// the normal Transition() guard would block the operation.
    /// </summary>
    public void ForceCancel() => Status = AppointmentStatus.Cancelled;

    /// <summary>
    /// Force-completes the appointment. Used when a doctor checks out
    /// and any InProgress appointments are auto-completed.
    /// </summary>
    public void ForceComplete() => Status = AppointmentStatus.Completed;

    /// <summary>
    /// Marks the appointment as NoShow. Used by delay handling (MarkMissed option).
    /// </summary>
    public void MarkNoShow() => Status = AppointmentStatus.NoShow;

    /// <summary>
    /// Resets a Waiting appointment back to Pending.
    /// Used when rescheduling — the patient hasn't been called yet on the new day.
    /// </summary>
    public void ResetToPending()
    {
        if (Status == AppointmentStatus.Waiting)
            Status = AppointmentStatus.Pending;
    }
}
