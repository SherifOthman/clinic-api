using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class ClinicBranch : AuditableTenantEntity
{
    public string Name { get; set; } = null!;
    public string AddressLine { get; set; } = null!;
    public int CountryGeoNameId { get; set; }
    public int StateGeoNameId { get; set; }
    public int CityGeoNameId { get; set; }
    public bool IsMainBranch { get; set; }
    public bool IsActive { get; set; } = true;
}
