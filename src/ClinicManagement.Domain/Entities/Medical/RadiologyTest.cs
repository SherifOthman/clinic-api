using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Radiology tests available at a clinic
/// </summary>
public class RadiologyTest : TenantEntity
{
    
    public string Name { get; set; } = null!; // X-Ray Chest
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
