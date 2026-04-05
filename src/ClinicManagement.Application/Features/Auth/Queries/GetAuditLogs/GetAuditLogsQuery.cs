using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Queries;

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
    int PageSize = 50
) : IRequest<Result<AuditLogsResponse>>;

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

public record AuditLogsResponse(
    List<AuditLogDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);
