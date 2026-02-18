using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class MedicalVisitRadiology : BaseEntity
{
    public int MedicalVisitId { get; set; }
    
    public int RadiologyTestId { get; set; }
    
    public string? Notes { get; set; }
}
