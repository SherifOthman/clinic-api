using ClinicManagement.API.Common;

namespace ClinicManagement.API.Entities;

/// <summary>
/// Junction entity for many-to-many relationship between Patient and ChronicDisease
/// </summary>
public class PatientChronicDisease : AuditableEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    
    public Guid ChronicDiseaseId { get; set; }
    public ChronicDisease ChronicDisease { get; set; } = null!;
}
