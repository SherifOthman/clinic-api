using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries;

public record GetPatientDetailQuery(Guid PatientId, bool IsSuperAdmin = false, string Lang = "en") : IRequest<Result<PatientDetailDto>>;

/// <summary>
/// Full patient detail DTO.
/// Location names are resolved server-side — no extra frontend API calls needed.
/// IDs are still included so the edit form can pre-populate the location selectors.
/// </summary>
public record PatientDetailDto
{
    public string Id { get; init; } = null!;
    public string PatientCode { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public DateOnly DateOfBirth { get; init; }
    public string Gender { get; init; } = null!;
    public string? BloodType { get; init; }
    // IDs for the edit form
    public int? CountryGeonameId { get; init; }
    public int? StateGeonameId { get; init; }
    public int? CityGeonameId { get; init; }
    // Resolved names for display
    public string? CountryName { get; init; }
    public string? StateName { get; init; }
    public string? CityName { get; init; }
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
