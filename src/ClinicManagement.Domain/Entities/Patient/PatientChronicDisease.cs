using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Junction entity for many-to-many relationship between Patient and ChronicDisease
/// </summary>
public class PatientChronicDisease : AuditableEntity
{
    public Guid PatientId { get; set; }
    
    public Guid ChronicDiseaseId { get; set; }
}
