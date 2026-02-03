using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Global Patient entity - represents the real person.
/// May or may not have an account (UserId nullable).
/// Minimal data - detailed data is in ClinicPatient per clinic.
/// </summary>
public class Patient : BaseEntity
{
    public Guid? UserId { get; set; } // Nullable - patient may not have account
    
    // Minimal global data
    public string? GlobalIdentifier { get; set; } // National ID, SSN, etc. (optional)
    
    // Navigation properties
    public User? User { get; set; }
    public ICollection<ClinicPatient> ClinicPatients { get; set; } = new List<ClinicPatient>();

    // Business logic moved to Domain Service
    // Use IPatientDomainService for any calculations
}
