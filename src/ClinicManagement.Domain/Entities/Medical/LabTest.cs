using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Laboratory tests available at a clinic
/// </summary>
public class LabTest : TenantEntity
{
    
    public string Name { get; set; } = null!; // CBC, ESR...
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
