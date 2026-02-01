using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class ClinicBranch : AuditableEntity
{
    public Guid ClinicId { get; set; }
    public string Name { get; set; } = string.Empty; // Branch name
    
    // GeoNames integration - replace database foreign keys with GeoNames data
    public int GeoNameId { get; set; } // GeoNames location ID
    public string CityName { get; set; } = string.Empty; // Cached city name for performance
    public string? StateName { get; set; } // Cached state/province name
    public string CountryCode { get; set; } = string.Empty; // ISO country code
    public decimal Latitude { get; set; } // Geographic coordinates
    public decimal Longitude { get; set; }
    
    public string Address { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual Clinic Clinic { get; set; } = null!;
    public virtual ICollection<ClinicBranchPhoneNumber> PhoneNumbers { get; set; } = new List<ClinicBranchPhoneNumber>();
}