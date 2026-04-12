using ClinicManagement.API.Models;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Features.Audit.Queries;
using ClinicManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

/// <summary>
/// SuperAdmin-only audit log endpoint.
/// Provides full cross-clinic audit trail for debugging and compliance.
/// </summary>
[Authorize(Policy = "SuperAdmin")]
[Route("api/audit")]
public class AuditController : BaseApiController
{
    /// <summary>
    /// Query audit logs across all clinics.
    /// Filter by clinic, user, entity type, action, or date range.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status403Forbidden)]
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
        var query = new GetAuditLogsQuery(entityType, entityId, action, from, to, userSearch, clinicSearch, pagination.PageNumber, pagination.PageSize);
        var result = await Sender.Send(query, cancellationToken);
        return HandleResult(result, "Failed to retrieve audit logs");
    }
}
