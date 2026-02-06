using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Nurse-specific information linked to a User account
/// </summary>
public class Nurse : AuditableEntity
{
    public Guid UserId { get; set; }
    public string? Certification { get; set; }
    public string? Department { get; set; }
    public ShiftType? ShiftType { get; set; }
    public bool CanAdministerMedication { get; set; } = true;

    // Navigation properties
    public virtual User User { get; set; } = null!;
}
