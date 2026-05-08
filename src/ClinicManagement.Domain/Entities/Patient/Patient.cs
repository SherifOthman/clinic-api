using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// A patient's medical record at a specific clinic.
/// </summary>
public class Patient : AuditableTenantEntity, ISoftDeletable, IAuditableEntity
{
    public string PatientCode { get; set; } = null!;
    public bool IsDeleted { get; set; } = false;

    public string FullName { get; set; } = null!;
    public Gender Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }

    public BloodType? BloodType { get; set; }

    public int? CountryGeonameId { get; set; }
    public int? StateGeonameId { get; set; }
    public int? CityGeonameId { get; set; }

    // Navigation
    public GeoCountry? Country { get; set; }
    public GeoState? State { get; set; }
    public GeoCity? City { get; set; }
    public ICollection<PatientPhone> Phones { get; set; } = new List<PatientPhone>();
    public ICollection<PatientChronicDisease> ChronicDiseases { get; set; } = new List<PatientChronicDisease>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    // ── Domain factory ────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new patient record with all required fields set consistently.
    /// Phones and chronic diseases are added separately via the repository helpers
    /// because they require normalisation and ID assignment after the patient is persisted.
    /// </summary>
    public static Patient Create(
        Guid clinicId,
        string patientCode,
        string fullName,
        Gender gender,
        DateOnly? dateOfBirth,
        BloodType? bloodType = null,
        int? countryGeonameId = null,
        int? stateGeonameId = null,
        int? cityGeonameId = null) => new()
    {
        ClinicId         = clinicId,
        PatientCode      = patientCode,
        FullName         = fullName.Trim(),
        Gender           = gender,
        DateOfBirth      = dateOfBirth,
        BloodType        = bloodType,
        CountryGeonameId = countryGeonameId,
        StateGeonameId   = stateGeonameId,
        CityGeonameId    = cityGeonameId,
        CreatedAt        = DateTimeOffset.UtcNow,
    };
}
