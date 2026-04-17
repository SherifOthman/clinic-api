using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Extra data that only doctors have.
/// Attached 1:1 to a ClinicMember whose Role = Doctor.
/// Schedule (working days + visit types) is per-branch via DoctorBranchSchedule.
/// </summary>
public class DoctorInfo : BaseEntity
{
    public Guid ClinicMemberId { get; init; }
    public Guid? SpecializationId { get; set; }
    public string? LicenseNumber { get; set; }

    /// <summary>When false, only the clinic owner can edit this doctor's schedule and visit types.</summary>
    public bool CanSelfManageSchedule { get; set; } = true;

    // Navigation
    public ClinicMember ClinicMember { get; set; } = null!;
    public Specialization? Specialization { get; set; }
    public ICollection<DoctorBranchSchedule> BranchSchedules { get; set; } = new List<DoctorBranchSchedule>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
