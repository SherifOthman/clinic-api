using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class PatientChronicDisease : BaseEntity
{
    public Guid PatientId { get; set; }
    public Guid ChronicDiseaseId { get; set; }
}
