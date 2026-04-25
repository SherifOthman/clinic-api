using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Commands;

public record UpdatePatientCommand(
    Guid Id,
    string FullName,
    string DateOfBirth,
    string Gender,
    int? CountryGeonameId,
    int? StateGeonameId,
    int? CityGeonameId,
    string? BloodType,
    List<string>? PhoneNumbers = null,
    List<Guid>? ChronicDiseaseIds = null
) : IRequest<Result>;
