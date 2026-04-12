namespace ClinicManagement.API.Contracts.Patients;

public record CreatePatientRequest(
    string FullName,
    string DateOfBirth,
    string Gender,
    string? CityNameEn,
    string? CityNameAr,
    string? StateNameEn,
    string? StateNameAr,
    string? CountryNameEn,
    string? CountryNameAr,
    string? BloodType,
    List<string> PhoneNumbers,
    List<Guid> ChronicDiseaseIds);

public record UpdatePatientRequest(
    string FullName,
    string DateOfBirth,
    string Gender,
    string? CityNameEn,
    string? CityNameAr,
    string? StateNameEn,
    string? StateNameAr,
    string? CountryNameEn,
    string? CountryNameAr,
    string? BloodType,
    List<string>? PhoneNumbers = null,
    List<Guid>? ChronicDiseaseIds = null);
