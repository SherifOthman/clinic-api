using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class ClinicBranch : AuditableTenantEntity
{
    public string Name { get; set; } = null!;
    public string? AddressLine { get; set; }
    public string? CityNameEn { get; set; }
    public string? CityNameAr { get; set; }
    public string? StateNameEn { get; set; }
    public string? StateNameAr { get; set; }
    public bool IsMainBranch { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ClinicBranchPhoneNumber> PhoneNumbers { get; set; } = new List<ClinicBranchPhoneNumber>();

    // Navigation properties
    public Clinic Clinic { get; set; } = null!;
}
