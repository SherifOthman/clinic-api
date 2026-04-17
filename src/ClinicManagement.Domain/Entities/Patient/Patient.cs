using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// A person's medical record at a specific clinic.
/// PersonId links to the Person entity (the real human).
/// Personal data (name, gender, DOB) is being migrated to Person.
/// </summary>
public class Patient : AuditableTenantEntity
{
    public string PatientCode { get; set; } = null!;

    // ── Kept during migration — will move to Person once fully adopted ──
    public string FullName { get; set; } = null!;
    public Gender Gender { get; set; }
    public DateOnly DateOfBirth { get; set; }

    /// <summary>
    /// Links this Patient to their Person record.
    /// Nullable during migration — will become required once all patients have a Person.
    /// </summary>
    public Guid? PersonId { get; set; }

    public BloodType? BloodType { get; set; }

    public int? CountryGeonameId { get; set; }
    public int? StateGeonameId { get; set; }
    public int? CityGeonameId { get; set; }

    // Navigation
    public Person? Person { get; set; }
    public GeoCountry? Country { get; set; }
    public GeoState? State { get; set; }
    public GeoCity? City { get; set; }
    public ICollection<PatientPhone> Phones { get; set; } = new List<PatientPhone>();
    public ICollection<PatientChronicDisease> ChronicDiseases { get; set; } = new List<PatientChronicDisease>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
