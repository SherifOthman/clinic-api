using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries;

public record GetPatientsQuery(
    string? SearchTerm,
    int PageNumber,
    int PageSize,
    string? SortBy,
    string SortDirection,
    string? Gender,
    string? ClinicSearch = null,
    int? StateGeonameId = null,
    int? CityGeonameId = null,
    int? CountryGeonameId = null,
    bool IsSuperAdmin = false
) : PaginatedQuery(PageNumber, PageSize), IRequest<Result<PaginatedResult<PatientDto>>>;

/// <summary>
/// List DTO — minimal fields for the patients table.
/// Location is returned as GeoNames IDs; the frontend resolves display names
/// from the GeoNames API using the user's current language.
/// </summary>
public record PatientDto
{
    public string Id { get; init; } = null!;
    public string PatientCode { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public DateOnly DateOfBirth { get; init; }
    public string Gender { get; init; } = null!;
    public string? BloodType { get; init; }
    public int ChronicDiseaseCount { get; init; }
    public string? PrimaryPhone { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public string? ClinicName { get; init; }
    public int? CountryGeonameId { get; init; }
    public int? StateGeonameId { get; init; }
    public int? CityGeonameId { get; init; }
}
