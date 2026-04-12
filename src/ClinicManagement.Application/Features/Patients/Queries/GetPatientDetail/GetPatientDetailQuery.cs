using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries;

public record GetPatientDetailQuery(Guid PatientId, bool IsSuperAdmin = false) : IRequest<Result<PatientDetailDto>>;

public record PatientDetailDto
{
    public string Id { get; init; } = null!;
    public string PatientCode { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public DateOnly DateOfBirth { get; init; }
    public string Gender { get; init; } = null!;
    public string? BloodType { get; init; }
    public string? CityNameEn { get; init; }
    public string? CityNameAr { get; init; }
    public string? StateNameEn { get; init; }
    public string? StateNameAr { get; init; }
    public string? CountryNameEn { get; init; }
    public string? CountryNameAr { get; init; }
    public List<string> PhoneNumbers { get; init; } = [];
    public List<PatientChronicDiseaseDto> ChronicDiseases { get; init; } = [];
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public string? CreatedBy { get; init; }
    public string? UpdatedBy { get; init; }
    public string? ClinicId { get; init; }
    public string? ClinicName { get; init; }
}

public record PatientChronicDiseaseDto(string Id, string NameEn, string NameAr);
