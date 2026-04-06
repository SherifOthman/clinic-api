using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries;

public record GetPatientsQuery(
    string? SearchTerm,
    int PageNumber,
    int PageSize,
    string? SortBy,
    string SortDirection,
    bool? IsMale,
    Guid? ClinicId = null,   // SuperAdmin only — filter by specific clinic
    bool IsSuperAdmin = false // when true, bypasses tenant filter
) : IRequest<Result<PaginatedPatientsResponse>>;

// List DTO — minimal fields, no nested collections for performance
public record PatientDto
{
    public string Id { get; init; } = null!;
    public string PatientCode { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public string DateOfBirth { get; init; } = null!;
    public bool IsMale { get; init; }
    public string? BloodType { get; init; }
    public string? PrimaryPhone { get; init; }
    public int PhoneCount { get; init; }
    public int ChronicDiseaseCount { get; init; }
    public string CreatedAt { get; init; } = null!;
    public string? ClinicName { get; init; }  // populated for SuperAdmin view
}

public record PaginatedPatientsResponse
{
    public List<PatientDto> Items { get; init; } = new();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}
