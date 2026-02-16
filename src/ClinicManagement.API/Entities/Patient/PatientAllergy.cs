using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Enums;

namespace ClinicManagement.API.Entities;

/// <summary>
/// Patient allergy - tracks drug and other allergies for patient safety
/// CRITICAL for prescription safety checks
/// </summary>
public class PatientAllergy : BaseEntity
{
    public Guid PatientId { get; set; }
    public string AllergyName { get; set; } = null!;  // Penicillin, Aspirin, Peanuts, etc.
    public AllergySeverity Severity { get; set; }
    public string? Reaction { get; set; }  // Rash, Anaphylaxis, Swelling, etc.
    public DateTime? DiagnosedAt { get; set; }
    public string? Notes { get; set; }
    
    // Navigation
    public Patient Patient { get; set; } = null!;
}
