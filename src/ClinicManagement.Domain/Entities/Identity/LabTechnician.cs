using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Lab technician-specific information linked to a User account
/// </summary>
public class LabTechnician : AuditableEntity
{
    public Guid UserId { get; set; }
    public string? Certification { get; set; }
    public string? Specialization { get; set; } // Blood, Radiology, etc.
    public bool CanApproveResults { get; set; } = false;

    // Navigation properties
    public virtual User User { get; set; } = null!;
}
