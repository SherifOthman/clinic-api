namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Pricing configuration per VisitType per ClinicBranch.
/// Composite PK: (ClinicBranchId, VisitTypeId).
/// Price is read from here and snapshotted into Appointment.Price at booking time.
/// </summary>
public class BranchVisitType
{
    public Guid ClinicBranchId { get; set; }
    public Guid VisitTypeId { get; set; }

    /// <summary>Price for this visit type at this branch.</summary>
    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public ClinicBranch Branch { get; set; } = null!;
    public VisitType VisitType { get; set; } = null!;
}
