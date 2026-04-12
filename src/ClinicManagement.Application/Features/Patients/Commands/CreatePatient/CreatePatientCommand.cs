using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Commands;

public record CreatePatientCommand(
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
    List<Guid> ChronicDiseaseIds
) : IRequest<Result<Guid>>;
