using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class ClinicBranch : AuditableTenantEntity, IAuditableEntity, ISoftDeletable
{
    public string Name { get; set; } = null!;
    public string? AddressLine { get; set; }

    /// <summary>GeoNames ID for the branch's state/governorate.</summary>
    public int? StateGeonameId { get; set; }

    /// <summary>GeoNames ID for the branch's city.</summary>
    public int? CityGeonameId { get; set; }

    public bool IsMainBranch { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public Clinic Clinic { get; set; } = null!;
    public ICollection<ClinicBranchPhoneNumber> PhoneNumbers { get; set; } = new List<ClinicBranchPhoneNumber>();
    public ICollection<Appointment> Appointment { get; set; } = new List<Appointment>();
    public ICollection<DoctorBranchSchedule> DoctorSchedules { get; set; } = new List<DoctorBranchSchedule>();

    // ── Domain factory ────────────────────────────────────────────────────────

    /// <summary>
    /// Creates the main branch during clinic onboarding.
    /// </summary>
    public static ClinicBranch CreateMain(
        Guid clinicId,
        string name,
        string? addressLine,
        int? stateGeonameId,
        int? cityGeonameId,
        IEnumerable<ClinicBranchPhoneNumber>? phoneNumbers = null) => new()
    {
        ClinicId       = clinicId,
        Name           = name,
        AddressLine    = addressLine,
        StateGeonameId = stateGeonameId,
        CityGeonameId  = cityGeonameId,
        IsMainBranch   = true,
        IsActive       = true,
        PhoneNumbers   = phoneNumbers?.ToList() ?? [],
    };
}
