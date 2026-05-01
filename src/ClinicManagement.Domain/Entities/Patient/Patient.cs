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

    // Navigation
    public Person Person { get; set; } = null!;
    public GeoCountry? Country { get; set; }
    public GeoState? State { get; set; }
    public GeoCity? City { get; set; }
    public ICollection<PatientPhone> Phones { get; set; } = new List<PatientPhone>();
    public ICollection<PatientChronicDisease> ChronicDiseases { get; set; } = new List<PatientChronicDisease>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
