namespace ClinicManagement.API.Contracts.Patients;

public record CreatePatientRequest(
    string FullName,
    string DateOfBirth,
    string Gender,
    int? CountryGeonameId,
    int? StateGeonameId,
    int? CityGeonameId,
    string? BloodType,
    List<string> PhoneNumbers,
    List<Guid> ChronicDiseaseIds);

public record UpdatePatientRequest(
    string FullName,
    string DateOfBirth,
    string Gender,
    int? CountryGeonameId,
    int? StateGeonameId,
    int? CityGeonameId,
    string? BloodType,
    List<string>? PhoneNumbers = null,
    List<Guid>? ChronicDiseaseIds = null);
