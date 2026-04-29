using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// A visit type offered by a doctor at a specific branch (e.g. Consultation, Follow-up).
/// Price is per-doctor-per-branch and snapshotted into Appointment at booking time.
/// Single Name field — no bilingual split; clinic staff enter it in their preferred language.
/// </summary>
public class VisitType : BaseEntity, ISoftDeletable
{
    public Guid DoctorBranchScheduleId { get; init; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public bool IsDeleted { get; set; } = false;
    public bool IsActive { get; set; } = true;

    // ── Computed ──────────────────────────────────────────────────────────────

    public bool IsFree => Price == 0;

    // Navigation
    public DoctorBranchSchedule Schedule { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
