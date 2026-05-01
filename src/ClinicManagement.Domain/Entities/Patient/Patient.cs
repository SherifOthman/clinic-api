using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// A person's medical record at a specific clinic.
/// All personal data (name, gender, DOB) lives on Person.
/// </summary>
public class Patient : AuditableTenantEntity, ISoftDeletable, IAuditableEntity
{
    public string PatientCode { get; set; } = null!;

    public bool IsDeleted { get; set; } = false;

    public Guid PersonId { get; set; }

    public BloodType? BloodType { get; set; }

    public int? CountryGeonameId { get; set; }
    public int? StateGeonameId { get; set; }
    public int? CityGeonameId { get; set; }

    // ── Computed ──────────────────────────────────────────────────────────────

    /// <summary>Age in full years, or null if DateOfBirth is not set.</summary>
    public int? Age
    {
        get
        {
            if (Person?.DateOfBirth is not { } dob) return null;
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var age = today.Year - dob.Year;
            if (dob.AddYears(age) > today) age--;
            return age;
        }
    }

    public bool HasChronicDiseases => ChronicDiseases.Count > 0;
    public bool HasPhones => Phones.Count > 0;
    public bool HasLocation => CountryGeonameId.HasValue;

    // Navigation
    public Person Person { get; set; } = null!;
    public GeoCountry? Country { get; set; }
    public GeoState? State { get; set; }
    public GeoCity? City { get; set; }
    public ICollection<PatientPhone> Phones { get; set; } = new List<PatientPhone>();
    public ICollection<PatientChronicDisease> ChronicDiseases { get; set; } = new List<PatientChronicDisease>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
