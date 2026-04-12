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
    string? StateSearch = null,
    string? CitySearch = null,
    string? CountrySearch = null,
    bool IsSuperAdmin = false
) : PaginatedQuery(PageNumber, PageSize), IRequest<Result<PaginatedResult<PatientDto>>>;

// List DTO — minimal fields, no nested collections for performance
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
    public string? ClinicName { get; init; }  // populated for SuperAdmin view
    public string? CityNameEn { get; init; }
    public string? CityNameAr { get; init; }
    public string? StateNameEn { get; init; }
    public string? StateNameAr { get; init; }
}
