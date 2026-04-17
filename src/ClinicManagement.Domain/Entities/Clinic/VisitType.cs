using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// A visit type offered by a doctor at a specific branch (e.g. كشف / إعادة).
/// Price is per-doctor-per-branch and snapshotted into Appointment at booking time.
/// </summary>
public class VisitType : BaseEntity
{
    public Guid DoctorBranchScheduleId { get; init; }
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public DoctorBranchSchedule Schedule { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
