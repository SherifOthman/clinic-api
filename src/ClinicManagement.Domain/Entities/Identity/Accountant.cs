using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Accountant-specific information linked to a User account
/// </summary>
public class Accountant : AuditableEntity
{
    public Guid UserId { get; set; }
    public string? CertificationNumber { get; set; }
    public bool CanApproveExpenses { get; set; } = false;
    public decimal? MaxApprovalAmount { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
}
