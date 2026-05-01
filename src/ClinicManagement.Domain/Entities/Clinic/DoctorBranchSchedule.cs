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
}
