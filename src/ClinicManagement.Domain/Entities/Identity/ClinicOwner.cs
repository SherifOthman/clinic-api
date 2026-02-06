using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Clinic owner-specific information linked to a User account
/// </summary>
public class ClinicOwner : AuditableEntity
{
    public Guid UserId { get; set; }
    public string? BusinessLicense { get; set; }
    public string? TaxId { get; set; }
    public decimal? OwnershipPercentage { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual User User { get; set; } = null!;
}
