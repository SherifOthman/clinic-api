using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class ClinicBranch : AuditableTenantEntity
{
    public string Name { get; set; } = null!;
    public string? AddressLine { get; set; }

    /// <summary>GeoNames ID for the branch's state/governorate.</summary>
    public int? StateGeonameId { get; set; }

    /// <summary>GeoNames ID for the branch's city.</summary>
    public int? CityGeonameId { get; set; }

    public bool IsMainBranch { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ClinicBranchPhoneNumber> PhoneNumbers { get; set; } = new List<ClinicBranchPhoneNumber>();

    // Navigation properties
    public Clinic Clinic { get; set; } = null!;
}
