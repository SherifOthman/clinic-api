using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// State/Governorate/Region entity - Snapshot from GeoNames
/// Represents administrative division level 1 (e.g., Cairo Governorate, Riyadh Region)
/// Uses integer ID to match GeoNames system
/// </summary>
public class State : ReferenceEntity
{
    /// <summary>
    /// GeoNames unique identifier - used for lazy seeding and deduplication
    /// </summary>
    public int GeonameId { get; set; }
    
    /// <summary>
    /// Foreign key to parent country
    /// </summary>
    public int CountryId { get; set; }
    public Country Country { get; set; } = null!;
    
    /// <summary>
    /// State name in English
    /// </summary>
    public string NameEn { get; set; } = null!;
    
    /// <summary>
    /// State name in Arabic
    /// </summary>
    public string NameAr { get; set; } = null!;
    
    // Navigation properties
    public ICollection<City> Cities { get; set; } = new List<City>();
}
