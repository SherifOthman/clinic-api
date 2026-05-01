using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class ClinicBranch : AuditableTenantEntity, IAuditableEntity
{
    public string Name { get; set; } = null!;
    public string? AddressLine { get; set; }

    /// <summary>GeoNames ID for the branch's state/governorate.</summary>
    public int? StateGeonameId { get; set; }

    /// <summary>GeoNames ID for the branch's city.</summary>
    public int? CityGeonameId { get; set; }

    public bool IsMainBranch { get; set; }
    public bool IsActive { get; set; } = true;

    // ── Computed ──────────────────────────────────────────────────────────────

    public bool HasAddress       => !string.IsNullOrWhiteSpace(AddressLine);
    public bool HasPhoneNumbers  => PhoneNumbers.Count > 0;
    public bool HasLocation      => StateGeonameId.HasValue && CityGeonameId.HasValue;

    // Navigation properties
    public Clinic Clinic { get; set; } = null!;
    public ICollection<ClinicBranchPhoneNumber> PhoneNumbers { get; set; } = new List<ClinicBranchPhoneNumber>();
    public ICollection<Appointment> Appointment { get; set; } = new List<Appointment>();
    public ICollection<DoctorBranchSchedule> DoctorSchedules { get; set; } = new List<DoctorBranchSchedule>();
}
