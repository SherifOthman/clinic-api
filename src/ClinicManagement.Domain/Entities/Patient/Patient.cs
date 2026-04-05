using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

public class Patient : AuditableTenantEntity
{
    public string PatientCode { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public bool IsMale { get; set; }
    public int? CountryGeoNameId { get; set; }
    public int? StateGeoNameId { get; set; }
    public int? CityGeoNameId { get; set; }
    public DateTime DateOfBirth { get; set; }
    public BloodType? BloodType { get; set; }

    // Navigation properties
    public ICollection<PatientPhone> Phones { get; set; } = new List<PatientPhone>();
    public ICollection<PatientChronicDisease> ChronicDiseases { get; set; } = new List<PatientChronicDisease>();
}
