using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Models.Filters;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Audit.Queries;

public record GetAuditLogsQuery(
    AuditLogFilter Filter,
    int PageNumber = 1,
    int PageSize   = 10
) : PaginatedQuery(PageNumber, PageSize), IRequest<Result<PaginatedResult<AuditLogDto>>>;

public record AuditLogDto(
    Guid Id,
    DateTimeOffset Timestamp,
    Guid? ClinicId,
    string? ClinicName,
    Guid? UserId,
    string? FullName,
    string? Username,
    string? UserEmail,
    string? UserRole,
    string? UserAgent,
    string EntityType,
    string EntityId,
    string Action,
    string? IpAddress,
    string? Changes
);
