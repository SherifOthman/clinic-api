using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Pharmacist-specific information linked to a User account
/// </summary>
public class Pharmacist : AuditableEntity
{
    public Guid UserId { get; set; }
    public string? LicenseNumber { get; set; }
    public string? Specialization { get; set; }
    public bool CanDispensePrescriptions { get; set; } = true;

    // Navigation properties
    public virtual User User { get; set; } = null!;
}
