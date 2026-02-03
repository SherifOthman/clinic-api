using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Phone numbers for ClinicPatient (clinic-specific).
/// IdentityUser has ONE primary verified phone for authentication.
/// ClinicPatient can have multiple phone numbers with one marked as primary.
/// </summary>
public class ClinicPatientPhoneNumber : BaseEntity
{
    public Guid ClinicPatientId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Label { get; set; } // e.g., "Home", "Mobile", "Work"
    public bool IsPrimary { get; set; } = false;
    
    // Navigation properties
    public ClinicPatient ClinicPatient { get; set; } = null!;
}
