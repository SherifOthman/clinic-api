using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class PatientPhone : BaseEntity
{
    public Guid ClinicPatientId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Label { get; set; } // e.g., "Home", "Mobile", "Work"
    
    // Navigation properties
    public ClinicPatient ClinicPatient { get; set; } = null!;
}
