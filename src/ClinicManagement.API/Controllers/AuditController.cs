using ClinicManagement.API.Models;
using ClinicManagement.API.Authorization;
using ClinicManagement.API.RateLimiting;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Models.Filters;
using ClinicManagement.Application.Features.Audit.Queries;
using ClinicManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicManagement.API.Controllers;

[Route("api/audit")]
[EnableRateLimiting(RateLimitPolicies.UserReads)]
public class AuditController : BaseApiController
{
    /// <summary>
    /// SuperAdmin: query audit logs across ALL clinics.
    /// Filter by clinic, user, entity type, action, or date range.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.SuperAdmin)]
    [ProducesResponseType(typeof(PaginatedResult<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? entityType,
        [FromQuery] string? entityId,
        [FromQuery] AuditAction? action,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] string? userSearch,
        [FromQuery] string? clinicSearch,
        [FromQuery] PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAuditLogsQuery(
            new(entityType, entityId, action, from, to, userSearch, clinicSearch),
            pagination.PageNumber, pagination.PageSize);
        var result = await Sender.Send(query, cancellationToken);
        return HandleResult(result, "Failed to retrieve audit logs");
    }

    /// <summary>
    /// Clinic Owner: query audit logs scoped to their own clinic only.
    /// Lets owners see what their staff did — who deleted a patient, who changed permissions, etc.
    /// The clinic scope is enforced server-side from the JWT — cannot be overridden by the caller.
    /// </summary>
    [HttpGet("my-clinic")]
    [Authorize(Policy = AuthorizationPolicies.ClinicOwner)]
    [ProducesResponseType(typeof(PaginatedResult<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyClinicAuditLogs(
        [FromQuery] string? entityType,
        [FromQuery] string? entityId,
        [FromQuery] AuditAction? action,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] string? userSearch,
        [FromQuery] PaginationRequest pagination,
        [FromServices] ICurrentUserService currentUser,
        CancellationToken cancellationToken = default)
    {
        var clinicId = currentUser.GetRequiredClinicId();

        var query = new GetClinicAuditLogsQuery(
            clinicId,
            new(entityType, entityId, action, from, to, userSearch, null),
            pagination.PageNumber, pagination.PageSize);

        var result = await Sender.Send(query, cancellationToken);
        return HandleResult(result, "Failed to retrieve audit logs");
    }
}
