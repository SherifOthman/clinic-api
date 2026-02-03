using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Clinic has its own medication catalog.
/// Can link to global Medication OR be clinic-defined.
/// Supports both global reference medications and clinic-specific medications.
/// </summary>
public class ClinicMedication : AuditableEntity
{
    public Guid ClinicId { get; set; }
    public Guid? GlobalMedicationId { get; set; } // Optional link to global medication
    
    // Medication details (can override global or be clinic-specific)
    public string Name { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? Strength { get; set; } // e.g., "500mg", "10mg/ml"
    public string? Form { get; set; } // e.g., "Tablet", "Capsule", "Syrup"
    public string? Manufacturer { get; set; }
    public string? Description { get; set; }
    
    // Clinic-specific settings
    public bool IsActive { get; set; } = true;
    public decimal? Price { get; set; } // If clinic sells medications
    public string? DefaultDosage { get; set; }
    public string? DefaultFrequency { get; set; }
    public string? DefaultInstructions { get; set; }
    
    // Navigation properties
    public Clinic Clinic { get; set; } = null!;
    public Medication? GlobalMedication { get; set; }
    public ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
}