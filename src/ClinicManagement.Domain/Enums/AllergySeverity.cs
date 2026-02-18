namespace ClinicManagement.Domain.Enums;

/// <summary>
/// Severity level of patient allergy
/// </summary>
public enum AllergySeverity
{
    Mild,              // Minor reaction (e.g., mild rash)
    Moderate,          // Moderate reaction (e.g., hives, itching)
    Severe,            // Severe reaction (e.g., difficulty breathing)
    LifeThreatening    // Life-threatening (e.g., anaphylaxis)
}
