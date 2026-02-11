using ClinicManagement.API.Common;

namespace ClinicManagement.API.Entities;

public class MedicalVisitRadiology : BaseEntity
{
    public Guid MedicalVisitId { get; set; }
    public MedicalVisit MedicalVisit { get; set; } = null!;
    
    public Guid RadiologyTestId { get; set; }
    public RadiologyTest RadiologyTest { get; set; } = null!;
    
    public string? Notes { get; set; }
}
