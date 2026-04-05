using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class PatientChronicDisease : AuditableEntity
{
    public Guid PatientId { get; set; }
    public Guid ChronicDiseaseId { get; set; }
}
