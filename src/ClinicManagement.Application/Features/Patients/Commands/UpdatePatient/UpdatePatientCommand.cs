using ClinicManagement.Application.Features.Patients.Queries;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Commands;

public record UpdatePatientCommand(
    Guid Id,
    string FullName,
    string DateOfBirth,
    string Gender,
    int? CountryGeoNameId,
    int? StateGeoNameId,
    int? CityGeoNameId,
    string? BloodType,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? EmergencyContactRelation,
    List<Guid>? ChronicDiseaseIds = null
) : IRequest<Result<PatientDto>>;
