using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Visit type defined by a doctor for a specific branch.
/// Each doctor sets their own visit types (e.g. كشف / إعادة) with their own prices per branch.
/// Composite PK: (DoctorId, ClinicBranchId, Id).
/// Price is snapshotted into Appointment.Price at booking time.
/// </summary>
public class DoctorVisitType : BaseEntity
{
    public Guid DoctorId { get; set; }
    public Guid ClinicBranchId { get; set; }

    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;

    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public Doctor Doctor { get; set; } = null!;
    public ClinicBranch Branch { get; set; } = null!;
}
