using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Links ClinicPatient to ChronicDiseases.
/// This is clinic-specific - each clinic maintains its own patient chronic disease records.
/// </summary>
public class PatientChronicDisease : BaseEntity
{
    public Guid ClinicPatientId { get; set; }
    public Guid ChronicDiseaseId { get; set; }
    public DateTime DiagnosedDate { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual ClinicPatient ClinicPatient { get; set; } = null!;
    public virtual ChronicDisease ChronicDisease { get; set; } = null!;
}