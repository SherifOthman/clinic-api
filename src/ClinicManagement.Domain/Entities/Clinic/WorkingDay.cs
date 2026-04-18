using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>A doctor's working hours on a specific day at a specific branch.</summary>
public class WorkingDay : BaseEntity, ISoftDeletable
{
    public Guid DoctorBranchScheduleId { get; init; }
    public DayOfWeek Day { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsDeleted { get; set; } = false;
    public bool IsAvailable { get; set; } = true;

    // ── Computed ──────────────────────────────────────────────────────────────

    public int DurationMinutes => (int)(EndTime - StartTime).TotalMinutes;

    public bool ContainsTime(TimeOnly time) => time >= StartTime && time < EndTime;

    public bool OverlapsWith(WorkingDay other) =>
        StartTime < other.EndTime && EndTime > other.StartTime;

    // Navigation
    public DoctorBranchSchedule Schedule { get; set; } = null!;
}
