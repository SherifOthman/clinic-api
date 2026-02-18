using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

public class Patient : TenantEntity
{
    private readonly List<PatientPhone> _phoneNumbers = [];
    private readonly List<PatientChronicDisease> _chronicDiseases = [];
    private readonly List<PatientAllergy> _allergies = [];

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
    

    public Clinic Clinic { get; set; } = null!;
    public IReadOnlyCollection<PatientPhone> PhoneNumbers => _phoneNumbers.AsReadOnly();
    public IReadOnlyCollection<PatientChronicDisease> ChronicDiseases => _chronicDiseases.AsReadOnly();
    public IReadOnlyCollection<PatientAllergy> Allergies => _allergies.AsReadOnly();
    public ICollection<MedicalFile> MedicalFiles { get; set; } = new List<MedicalFile>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    
    // Calculated properties
    public int Age => DateTime.UtcNow.Year - DateOfBirth.Year - 
        (DateTime.UtcNow.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
    public bool IsAdult => Age >= 18;
    public bool IsMinor => Age < 18;
    public bool IsSenior => Age >= 65;
    public string PrimaryPhoneNumber => _phoneNumbers.FirstOrDefault(p => p.IsPrimary)?.PhoneNumber 
        ?? _phoneNumbers.FirstOrDefault()?.PhoneNumber 
        ?? string.Empty;
    public bool HasChronicDiseases => _chronicDiseases.Any();
    public int ChronicDiseaseCount => _chronicDiseases.Count;
}
