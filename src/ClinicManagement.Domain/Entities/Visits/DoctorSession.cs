using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Tracks a doctor's actual attendance for a specific date at a branch.
/// Created when the doctor checks in. Used to detect delays and trigger
/// delay-handling workflows.
///
/// Design: one session per doctor per branch per date.
/// </summary>
public class DoctorSession : AuditableTenantEntity
{
    public Guid DoctorInfoId { get; set; }
    public Guid BranchId { get; set; }
    public DateOnly Date { get; set; }

    /// <summary>When the doctor actually checked in (arrived).</summary>
    public DateTimeOffset? CheckedInAt { get; set; }

    /// <summary>When the doctor ended their session.</summary>
    public DateTimeOffset? CheckedOutAt { get; set; }

    /// <summary>Scheduled start time from working days config.</summary>
    public TimeOnly? ScheduledStartTime { get; set; }

    /// <summary>
    /// How the clinic chose to handle the delay.
    /// Null = no delay or not yet decided.
    /// </summary>
    public DelayHandlingOption? DelayHandling { get; set; }

    // ── Computed ──────────────────────────────────────────────────────────────

    public bool IsActive => CheckedInAt.HasValue && !CheckedOutAt.HasValue;

    /// <summary>Delay in minutes. Positive = late. Null if no scheduled time or not checked in.</summary>
    public int? DelayMinutes
    {
        get
        {
            if (!CheckedInAt.HasValue || !ScheduledStartTime.HasValue) return null;
            var scheduled = Date.ToDateTime(ScheduledStartTime.Value, DateTimeKind.Local);
            var actual    = CheckedInAt.Value.LocalDateTime;
            var diff      = (int)(actual - scheduled).TotalMinutes;
            return diff > 0 ? diff : null; // only positive = late
        }
    }

    public bool IsLate => DelayMinutes.HasValue && DelayMinutes > 0;

    // Navigation
    public DoctorInfo Doctor { get; set; } = null!;
    public ClinicBranch Branch { get; set; } = null!;
}

public enum DelayHandlingOption
{
    /// <summary>Shift all pending appointments forward by the delay duration.</summary>
    AutoShift,

    /// <summary>Mark all past appointments as NoShow.</summary>
    MarkMissed,

    /// <summary>Receptionist handles each appointment manually.</summary>
    Manual,
}
