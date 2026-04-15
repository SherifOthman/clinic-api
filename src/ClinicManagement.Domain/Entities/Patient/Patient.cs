using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

public class Patient : AuditableTenantEntity
{
    public string PatientCode { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public Gender Gender { get; set; }

    /// <summary>GeoNames ID for the patient's country (e.g. 357994 for Egypt).</summary>
    public int? CountryGeonameId { get; set; }

    /// <summary>GeoNames ID for the patient's state/governorate.</summary>
    public int? StateGeonameId { get; set; }

    /// <summary>GeoNames ID for the patient's city.</summary>
    public int? CityGeonameId { get; set; }

    public DateOnly DateOfBirth { get; set; }
    public BloodType? BloodType { get; set; }

    // Navigation properties
    public GeoCountry? Country { get; set; }
    public GeoState?   State   { get; set; }
    public GeoCity?    City    { get; set; }
    public ICollection<PatientPhone> Phones { get; set; } = new List<PatientPhone>();
    public ICollection<PatientChronicDisease> ChronicDiseases { get; set; } = new List<PatientChronicDisease>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
