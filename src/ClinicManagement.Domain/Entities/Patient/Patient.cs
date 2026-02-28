using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

public class Patient : AuditableTenantEntity
{
    public string PatientCode { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public bool IsMale { get; set; }
    public int? CityGeoNameId { get; set; }
    public DateTime DateOfBirth { get; set; }
    public BloodType? BloodType { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
    
    // Navigation properties
    public Clinic Clinic { get; set; } = null!;
    public ICollection<PatientPhone> Phones { get; set; } = new List<PatientPhone>();
    public ICollection<PatientAllergy> Allergies { get; set; } = new List<PatientAllergy>();
    public ICollection<PatientChronicDisease> ChronicDiseases { get; set; } = new List<PatientChronicDisease>();

    public int GetAge(DateTime now) => now.Year - DateOfBirth.Year - 
        (now.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
    public bool IsAdult(DateTime now) => GetAge(now) >= 18;
    public bool IsMinor(DateTime now) => GetAge(now) < 18;
    public bool IsSenior(DateTime now) => GetAge(now) >= 65;
    public bool HasEmergencyContact => !string.IsNullOrWhiteSpace(EmergencyContactName);
    public bool HasAllergies => Allergies.Any();
}
