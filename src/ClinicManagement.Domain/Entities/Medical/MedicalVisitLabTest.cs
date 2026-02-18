using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class MedicalVisitLabTest : BaseEntity
{
    public Guid MedicalVisitId { get; set; }
    
    public Guid LabTestId { get; set; }
    
    public string? Notes { get; set; }
}
