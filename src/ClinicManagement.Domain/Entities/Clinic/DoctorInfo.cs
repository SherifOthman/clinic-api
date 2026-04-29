using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Extra data that only doctors have.
/// Attached 1:1 to a ClinicMember whose Role = Doctor.
/// Schedule (working days + visit types) is per-branch via DoctorBranchSchedule.
/// </summary>
public class DoctorInfo : BaseEntity, ISoftDeletable
{
    public Guid ClinicMemberId { get; init; }
    public Guid? SpecializationId { get; set; }
    public string? LicenseNumber { get; set; }
    public bool IsDeleted { get; set; } = false;

    /// <summary>When false, only the clinic owner can edit this doctor's schedule and visit types.</summary>
    public bool CanSelfManageSchedule { get; set; } = true;

    /// <summary>
    /// How this doctor handles appointments.
    /// Queue = patients arrive and get a queue number (no fixed time).
    /// Time  = patients book a specific date + time slot.
    /// </summary>
    public AppointmentType AppointmentType { get; set; } = AppointmentType.Queue;

    /// <summary>
    /// Default visit duration in minutes for time-based appointments.
    /// Used to auto-calculate endTime when booking. Can be overridden per appointment.
    /// </summary>
    public int DefaultVisitDurationMinutes { get; set; } = 30;

    // ── Computed ──────────────────────────────────────────────────────────────

    public bool HasSpecialization => SpecializationId.HasValue;
    public bool HasLicense        => !string.IsNullOrWhiteSpace(LicenseNumber);
    public bool IsAvailableAtBranch(Guid branchId) =>
        BranchSchedules.Any(s => s.BranchId == branchId && s.IsActive);

    // Navigation
    public ClinicMember ClinicMember { get; set; } = null!;
    public Specialization? Specialization { get; set; }
    public ICollection<DoctorBranchSchedule> BranchSchedules { get; set; } = new List<DoctorBranchSchedule>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
