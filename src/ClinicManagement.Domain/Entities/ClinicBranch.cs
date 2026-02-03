using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class ClinicBranch : BaseEntity
{
    public Guid ClinicId { get; set; }
    public string Name { get; set; } = string.Empty; // Branch name
    public int CityId { get; set; } // GeoNames location ID
    
    public string Address { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual Clinic Clinic { get; set; } = null!;
    public virtual ICollection<ClinicBranchPhoneNumber> PhoneNumbers { get; set; } = new List<ClinicBranchPhoneNumber>();
    public List<Appointment> Appointments { get; set; } = new();

}