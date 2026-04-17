using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries;

public record GetPatientDetailQuery(Guid PatientId, bool IsSuperAdmin = false) : IRequest<Result<PatientDetailDto>>;

/// <summary>
/// Full patient detail DTO.
/// Both EN and AR location names are always returned — no re-fetching on language switch.
/// IDs are still included so the edit form can pre-populate the location selectors.
/// </summary>
public record PatientDetailDto
{
    public string Id { get; init; } = null!;
    public string PatientCode { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public DateOnly? DateOfBirth { get; init; }
    public string Gender { get; init; } = null!;
    public string? BloodType { get; init; }
    // IDs for the edit form
    public int? CountryGeonameId { get; init; }
    public int? StateGeonameId { get; init; }
    public int? CityGeonameId { get; init; }
    // Both language names — frontend picks based on current language
    public string? CountryNameEn { get; init; }
    public string? CountryNameAr { get; init; }
    public string? StateNameEn { get; init; }
    public string? StateNameAr { get; init; }
    public string? CityNameEn { get; init; }
    public string? CityNameAr { get; init; }
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
