using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Country entity - Snapshot from GeoNames
/// GeoNames is used as external provider only, not as primary data source
/// Uses integer ID to match GeoNames system
/// </summary>
public class Country : ReferenceEntity
{
    /// <summary>
    /// GeoNames unique identifier - used for lazy seeding and deduplication
    /// </summary>
    public int GeonameId { get; set; }
    
    /// <summary>
    /// ISO 3166-1 alpha-2 code (e.g., "EG", "SA", "US")
    /// </summary>
    public string Iso2Code { get; set; } = null!;
    
    /// <summary>
    /// International phone code (e.g., "+20", "+966", "+1")
    /// </summary>
    public string PhoneCode { get; set; } = null!;
    
    /// <summary>
    /// Country name in English
    /// </summary>
    public string NameEn { get; set; } = null!;
    
    /// <summary>
    /// Country name in Arabic
    /// </summary>
    public string NameAr { get; set; } = null!;
    
    // Navigation properties
    public ICollection<State> States { get; set; } = new List<State>();
}
