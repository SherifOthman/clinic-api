using ClinicManagement.API.Common;

namespace ClinicManagement.API.Entities;

/// <summary>
/// Receptionist-specific information linked to a User account
/// </summary>
public class Receptionist : AuditableEntity
{
    public Guid UserId { get; set; }
    public DateTime? HireDate { get; set; }
    public string? ShiftPreference { get; set; }
    public bool CanHandlePayments { get; set; } = true;
    public string? Languages { get; set; } // Comma-separated list

    // Navigation properties
    public virtual User User { get; set; } = null!;
}
