using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>A doctor's working hours on a specific day at a specific branch.</summary>
public class WorkingDay : BaseEntity
{
    public Guid DoctorBranchScheduleId { get; init; }
    public DayOfWeek Day { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsAvailable { get; set; } = true;

    // Navigation
    public DoctorBranchSchedule Schedule { get; set; } = null!;
}
