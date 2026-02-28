using ClinicManagement.Application.Features.Patients.Queries;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Commands;

public record CreatePatientCommand(
    string FullName,
    string DateOfBirth,
    string Gender,
    int? CityGeoNameId,
    string? BloodType,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? EmergencyContactRelation,
    List<PhoneNumberDto> PhoneNumbers,
    List<Guid> ChronicDiseaseIds
) : IRequest<Result<PatientDto>>;

public record PhoneNumberDto(
    string PhoneNumber,
    bool IsPrimary);
