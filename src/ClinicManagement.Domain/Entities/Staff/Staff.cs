using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Represents a staff member's association with a clinic.
/// All clinic-related users (ClinicOwner, Doctor, Receptionist) have a Staff record.
/// </summary>
public class Staff : TenantEntity
{
    public Guid UserId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
}
