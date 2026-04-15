using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Defines the type of visit — e.g. كشف (Initial/Consultation) or إعادة (Follow-up).
/// Each branch sets its own price per VisitType via BranchVisitType.
/// </summary>
public class VisitType : BaseEntity
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<BranchVisitType> BranchVisitTypes { get; set; } = new List<BranchVisitType>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
