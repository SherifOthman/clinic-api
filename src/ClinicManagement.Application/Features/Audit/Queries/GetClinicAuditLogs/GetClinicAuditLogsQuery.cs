using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Models.Filters;
using ClinicManagement.Application.Features.Audit.Queries;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Audit.Queries;

/// <summary>
/// Returns audit logs scoped to a specific clinic — used by Clinic Owners.
/// The clinicId is injected server-side from the JWT, never from the request body.
/// This prevents a clinic owner from querying another clinic's audit log.
/// </summary>
public record GetClinicAuditLogsQuery(
    Guid ClinicId,
    AuditLogFilter Filter,
    int PageNumber = 1,
    int PageSize   = 20
) : PaginatedQuery(PageNumber, PageSize), IRequest<Result<PaginatedResult<AuditLogDto>>>;
