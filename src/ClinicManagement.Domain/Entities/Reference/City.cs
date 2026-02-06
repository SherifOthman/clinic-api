using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// City entity - Snapshot from GeoNames
/// Represents cities/towns within a state
/// Uses integer ID to match GeoNames system
/// </summary>
public class City : ReferenceEntity
{
    /// <summary>
    /// GeoNames unique identifier - used for lazy seeding and deduplication
    /// </summary>
    public int GeonameId { get; set; }
    
    /// <summary>
    /// Foreign key to parent state
    /// </summary>
    public int StateId { get; set; }
    public State State { get; set; } = null!;
    
    /// <summary>
    /// City name in English
    /// </summary>
    public string NameEn { get; set; } = null!;
    
    /// <summary>
    /// City name in Arabic
    /// </summary>
    public string NameAr { get; set; } = null!;
}
