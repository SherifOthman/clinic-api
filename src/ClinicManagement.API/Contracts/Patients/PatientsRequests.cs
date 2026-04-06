using ClinicManagement.Application.Features.Patients.Commands;

namespace ClinicManagement.API.Contracts.Patients;

public record CreatePatientRequest(
    string FullName,
    string DateOfBirth,
    string Gender,
    int? CountryGeoNameId,
    int? StateGeoNameId,
    int? CityGeoNameId,
    string? BloodType,
    List<PhoneNumberDto> PhoneNumbers,
    List<Guid> ChronicDiseaseIds);

public record UpdatePatientRequest(
    string FullName,
    string DateOfBirth,
    string Gender,
    int? CountryGeoNameId,
    int? StateGeoNameId,
    int? CityGeoNameId,
    string? BloodType,
    List<Guid>? ChronicDiseaseIds = null);
