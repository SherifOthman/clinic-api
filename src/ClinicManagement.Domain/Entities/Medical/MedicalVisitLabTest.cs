using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class MedicalVisitLabTest : BaseEntity
{
    public int MedicalVisitId { get; set; }
    
    public int LabTestId { get; set; }
    
    public string? Notes { get; set; }
}
