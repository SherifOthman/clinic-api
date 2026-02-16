using ClinicManagement.API.Common;

namespace ClinicManagement.API.Entities;

public class MedicalVisitLabTest : BaseEntity
{
    public Guid MedicalVisitId { get; set; }
    public MedicalVisit MedicalVisit { get; set; } = null!;
    
    public Guid LabTestId { get; set; }
    public LabTest LabTest { get; set; } = null!;
    
    public string? Notes { get; set; }
}
