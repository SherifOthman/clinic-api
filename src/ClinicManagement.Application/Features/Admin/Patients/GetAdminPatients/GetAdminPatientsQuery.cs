using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Models.Filters;
using ClinicManagement.Application.Features.Patients.Queries;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Admin.Patients;

/// <summary>
/// Cross-tenant patient list — SuperAdmin only.
/// No IsSuperAdmin flag — this query always ignores the tenant filter.
/// </summary>
public record GetAdminPatientsQuery(
    AdminPatientFilter Filter,
    int PageNumber = 1,
    int PageSize   = 10
) : PaginatedQuery(PageNumber, PageSize), IRequest<Result<PaginatedResult<PatientDto>>>;
