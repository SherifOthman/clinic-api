using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Audit.Queries;

public record GetAuditLogsQuery(
    Guid? ClinicId = null,
    Guid? UserId = null,
    string? EntityType = null,
    string? EntityId = null,
    AuditAction? Action = null,
    DateTime? From = null,
    DateTime? To = null,
    string? UserSearch = null,
    string? ClinicSearch = null,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<Result<PaginatedResult<AuditLogDto>>>;

public record AuditLogDto(
    Guid Id,
    DateTime Timestamp,
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
