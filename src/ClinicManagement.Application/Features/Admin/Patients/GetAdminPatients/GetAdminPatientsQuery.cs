using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Features.Patients.Queries;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Admin.Patients;

/// <summary>
/// Cross-tenant patient list — SuperAdmin only.
/// No IsSuperAdmin flag — this query always ignores the tenant filter.
/// </summary>
public record GetAdminPatientsQuery(
    string? SearchTerm,
    int PageNumber,
    int PageSize,
    string? SortBy,
    string SortDirection,
    string? Gender,
    string? ClinicSearch = null,
    int? StateGeonameId = null,
    int? CityGeonameId = null,
    int? CountryGeonameId = null
) : PaginatedQuery(PageNumber, PageSize), IRequest<Result<PaginatedResult<PatientDto>>>;
