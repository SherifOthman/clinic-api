using ClinicManagement.API.Common;

namespace ClinicManagement.API.Entities;

/// <summary>
/// Radiology tests available at a clinic
/// </summary>
public class RadiologyTest : TenantEntity
{
    public Clinic Clinic { get; set; } = null!;
    
    public string Name { get; set; } = null!; // X-Ray Chest
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ICollection<MedicalVisitRadiology> MedicalVisitRadiologies { get; set; } = new List<MedicalVisitRadiology>();
}
