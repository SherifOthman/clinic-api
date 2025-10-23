using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Governorate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int CountryId { get; set; }
    
    // Navigation properties
    public virtual Country Country { get; set; } = null!;
    public virtual ICollection<City> Cities { get; set; } = new List<City>();
}
