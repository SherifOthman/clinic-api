using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// A doctor's schedule at a specific branch.
/// Groups working days and visit types together per branch.
/// A doctor can have different schedules and prices at different branches.
/// </summary>
public class DoctorBranchSchedule : BaseEntity, ISoftDeletable
{
    public Guid DoctorInfoId { get; init; }
    public Guid BranchId { get; init; }
    public bool IsDeleted { get; set; } = false;
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// How this doctor handles appointments at this specific branch.
    /// Queue = patients arrive and get a queue number (no fixed time).
    /// Time  = patients book a specific date + time slot.
    /// Defaults to the doctor's clinic-wide default when the schedule is first created.
    /// </summary>
    public AppointmentType AppointmentType { get; set; } = AppointmentType.Queue;

    // Navigation
    public DoctorInfo DoctorInfo { get; set; } = null!;
    public ClinicBranch Branch { get; set; } = null!;
    public ICollection<WorkingDay> WorkingDays { get; set; } = new List<WorkingDay>();
    public ICollection<VisitType> VisitTypes { get; set; } = new List<VisitType>();

    // ── Domain behaviour ──────────────────────────────────────────────────────

    /// <summary>
    /// Finds the next available working day after the given date, within 60 days.
    /// Returns null if no working day is found within the window.
    /// Moved from RescheduleDoctorHandler — business logic belongs in the domain.
    /// </summary>
    public DateOnly? FindNextWorkingDay(DateOnly afterDate)
    {
        var availableDays = WorkingDays
            .Where(w => w.IsAvailable)
            .Select(w => w.Day)
            .ToHashSet();

        if (availableDays.Count == 0) return null;

        var candidate = afterDate.AddDays(1);
        var limit     = afterDate.AddDays(60);

        while (candidate <= limit)
        {
            if (availableDays.Contains(candidate.DayOfWeek))
                return candidate;
            candidate = candidate.AddDays(1);
        }

        return null;
    }
}
