namespace ClinicManagement.API.Contracts.Patients;

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
