using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

public class Patient : TenantEntity
{
    public string PatientCode { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public bool IsMale { get; set; }
    public int? CityGeoNameId { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? BloodType { get; set; }
    public string? KnownAllergies { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }

    public int GetAge(DateTime now) => now.Year - DateOfBirth.Year - 
        (now.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
    public bool IsAdult(DateTime now) => GetAge(now) >= 18;
    public bool IsMinor(DateTime now) => GetAge(now) < 18;
    public bool IsSenior(DateTime now) => GetAge(now) >= 65;
    public bool HasEmergencyContact => !string.IsNullOrWhiteSpace(EmergencyContactName);
    public bool HasAllergies => !string.IsNullOrWhiteSpace(KnownAllergies);
}
