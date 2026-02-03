using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Junction entity for many-to-many relationship between ClinicPatient and ChronicDisease
/// </summary>
public class ClinicPatientChronicDisease : AuditableEntity
{
    public Guid ClinicPatientId { get; set; }
    public ClinicPatient ClinicPatient { get; set; } = null!;
    
    public Guid ChronicDiseaseId { get; set; }
    public ChronicDisease ChronicDisease { get; set; } = null!;
    
    /// <summary>
    /// Date when the chronic disease was diagnosed for this patient
    /// </summary>
    public DateTime? DiagnosedDate { get; set; }
    
    /// <summary>
    /// Current status of the chronic disease for this patient
    /// </summary>
    public string? Status { get; set; }
    
    /// <summary>
    /// Additional notes about this chronic disease for this patient
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Whether this chronic disease is currently active for the patient
    /// </summary>
    public bool IsActive { get; set; } = true;
}