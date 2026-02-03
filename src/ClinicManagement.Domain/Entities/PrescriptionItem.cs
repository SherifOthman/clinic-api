using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Items in a prescription: medications, lab requests, radiology requests.
/// Each item contains specific instructions.
/// </summary>
public class PrescriptionItem : AuditableEntity
{
    public Guid PrescriptionId { get; set; }
    public PrescriptionItemType ItemType { get; set; } // Medication, LabTest, Radiology
    
    // For medications
    public Guid? ClinicMedicationId { get; set; }
    
    // For lab/radiology
    public string? TestName { get; set; }
    public string? TestCode { get; set; }
    
    // Common fields
    public string ItemName { get; set; } = string.Empty;
    public string? Instructions { get; set; }
    public string? Dosage { get; set; } // For medications
    public string? Frequency { get; set; } // For medications
    public int? Duration { get; set; } // Days for medications
    public int? Quantity { get; set; }
    
    public bool IncludeInPrintout { get; set; } = true; // Doctor can choose what appears on prescription
    
    // Navigation properties
    public Prescription Prescription { get; set; } = null!;
    public ClinicMedication? ClinicMedication { get; set; }
}