using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

public class Patient : AuditableTenantEntity
{
    public string PatientCode { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public Gender Gender { get; set; }
    public string? CityNameEn { get; set; }
    public string? CityNameAr { get; set; }
    public string? StateNameEn { get; set; }
    public string? StateNameAr { get; set; }
    public string? CountryNameEn { get; set; }
    public string? CountryNameAr { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public BloodType? BloodType { get; set; }

    // Navigation properties
    public ICollection<PatientPhone> Phones { get; set; } = new List<PatientPhone>();
    public ICollection<PatientChronicDisease> ChronicDiseases { get; set; } = new List<PatientChronicDisease>();
}
