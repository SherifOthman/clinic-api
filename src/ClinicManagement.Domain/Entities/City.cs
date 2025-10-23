using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class City : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int GovernorateId { get; set; }
    
    // Navigation properties
    public virtual Governorate Governorate { get; set; } = null!;
    public virtual ICollection<ClinicBranch> ClinicBranches { get; set; } = new List<ClinicBranch>();
}
