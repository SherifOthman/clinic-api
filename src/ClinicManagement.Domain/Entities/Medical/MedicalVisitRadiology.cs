using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class MedicalVisitRadiology : BaseEntity
{
    public Guid MedicalVisitId { get; set; }
    
    public Guid RadiologyTestId { get; set; }
    
    public string? Notes { get; set; }
}
