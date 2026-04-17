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

    // Navigation properties
    public Clinic Clinic { get; set; } = null!;
    public ICollection<ClinicBranchPhoneNumber> PhoneNumbers { get; set; } = new List<ClinicBranchPhoneNumber>();
    public ICollection<Appointment> Appointment { get; set; } = new List<Appointment>();
    public ICollection<DoctorVisitType> DoctorVisitTypes { get; set; } = new List<DoctorVisitType>();
    public ICollection<DoctorBranchSchedule> DoctorSchedules { get; set; } = new List<DoctorBranchSchedule>();

}
