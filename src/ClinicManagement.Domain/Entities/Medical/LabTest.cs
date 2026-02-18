using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Laboratory tests available at a clinic
/// </summary>
public class LabTest : TenantEntity
{
    public Clinic Clinic { get; set; } = null!;
    
    public string Name { get; set; } = null!; // CBC, ESR...
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    

    public ICollection<MedicalVisitLabTest> MedicalVisitLabTests { get; set; } = new List<MedicalVisitLabTest>();
}
