using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Junction entity for many-to-many relationship between Patient and ChronicDisease
/// </summary>
public class PatientChronicDisease : AuditableEntity
{
    public int PatientId { get; set; }
    
    public int ChronicDiseaseId { get; set; }
}
