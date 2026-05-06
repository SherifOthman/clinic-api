using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Tracks a doctor's actual attendance for a specific date at a branch.
/// Created when the doctor checks in. Used to detect delays and trigger
/// delay-handling workflows.
///
/// Design: one session per doctor per branch per date.
///
/// Timezone strategy: everything is stored as UTC DateTimeOffset.
/// ScheduledStartUtc = the exact UTC moment the doctor was supposed to start.
/// CheckedInAt       = the exact UTC moment the doctor actually checked in.
/// Delay             = CheckedInAt - ScheduledStartUtc (pure UTC arithmetic, no timezone needed).
/// Display           = frontend converts UTC to local for display only.
/// </summary>
public class DoctorSession : AuditableTenantEntity
{
    public Guid DoctorInfoId { get; set; }
    public Guid BranchId { get; set; }
    public DateOnly Date { get; set; }

    /// <summary>When the doctor actually checked in (arrived). UTC.</summary>
    public DateTimeOffset? CheckedInAt { get; set; }

    /// <summary>When the doctor ended their session. UTC.</summary>
    public DateTimeOffset? CheckedOutAt { get; set; }

    /// <summary>
    /// The exact UTC moment the doctor was scheduled to start.
    /// Built at check-in time from: today's date (UTC) + WorkingDay.StartTime (local clock)
    /// converted to UTC using the server's local timezone offset.
    /// Stored as absolute UTC so delay calculation is timezone-independent.
    /// </summary>
    public DateTimeOffset? ScheduledStartUtc { get; set; }

    /// <summary>
    /// The local clock time as configured (e.g. "09:00") — kept for display only.
    /// Use ScheduledStartUtc for all calculations.
    /// </summary>
    public TimeOnly? ScheduledStartTime { get; set; }

    /// <summary>How the clinic chose to handle the delay. Null = no delay or not yet decided.</summary>
    public DelayHandlingOption? DelayHandling { get; set; }

    /// <summary>
    /// Delay in minutes, computed once at check-in and stored.
    /// Always use this — never recompute from timestamps.
    /// </summary>
    public int? StoredDelayMinutes { get; set; }

    // ── Computed ──────────────────────────────────────────────────────────────

    public bool IsActive => CheckedInAt.HasValue && !CheckedOutAt.HasValue;

    /// <summary>
    /// Delay in minutes using pure UTC arithmetic.
    /// Only called once at check-in to populate StoredDelayMinutes.
    /// After that, use StoredDelayMinutes directly.
    /// </summary>
    public int? DelayMinutes
    {
        get
        {
            if (!CheckedInAt.HasValue || !ScheduledStartUtc.HasValue) return null;
            var diff = (int)(CheckedInAt.Value.ToUniversalTime() - ScheduledStartUtc.Value.ToUniversalTime()).TotalMinutes;
            return diff > 0 ? diff : null;
        }
    }

    public bool IsLate => StoredDelayMinutes.HasValue && StoredDelayMinutes > 0;

    // Navigation
    public DoctorInfo Doctor { get; set; } = null!;
    public ClinicBranch Branch { get; set; } = null!;
}

public enum DelayHandlingOption
{
    /// <summary>Shift past-pending appointments to start from now, future ones by the same delay.</summary>
    AutoShift,

    /// <summary>Mark all past-pending appointments as NoShow.</summary>
    MarkMissed,

    /// <summary>Receptionist handles each appointment manually.</summary>
    Manual,

    /// <summary>Cancel the check-in — removes the session as if it never happened.</summary>
    Cancel,
}
