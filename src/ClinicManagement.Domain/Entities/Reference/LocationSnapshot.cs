using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Read-only cache of location names from external provider (GeoNames)
/// NOT a source of truth - only for display purposes
/// </summary>
public class LocationSnapshot : BaseEntity
{
    public int GeoNameId { get; set; }
    
    public LocationType Type { get; set; }
    
    public string NameEn { get; set; } = null!;
    
    public string NameAr { get; set; } = null!;
    
    public string Provider { get; set; } = "GeoNames";
    
    public DateTime LastSyncedAt { get; set; }
}

public enum LocationType
{
    Country = 1,
    State = 2,
    City = 3
}
