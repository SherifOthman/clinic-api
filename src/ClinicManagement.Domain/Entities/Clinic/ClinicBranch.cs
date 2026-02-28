using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class ClinicBranch : TenantEntity
{
    public string Name { get; set; } = null!;
    public string AddressLine { get; set; } = null!;
    public int CountryGeoNameId { get; set; }
    public int StateGeoNameId { get; set; }
    public int CityGeoNameId { get; set; }
    public bool IsMainBranch { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; private set; }
    public bool IsDeleted { get; set; }
    
    // Navigation properties
    public Clinic Clinic { get; set; } = null!;
    public ICollection<ClinicBranchPhoneNumber> PhoneNumbers { get; set; } = new List<ClinicBranchPhoneNumber>();
    public ICollection<ClinicBranchAppointmentPrice> AppointmentPrices { get; set; } = new List<ClinicBranchAppointmentPrice>();
    public ICollection<StaffBranch> StaffBranches { get; set; } = new List<StaffBranch>();
}
