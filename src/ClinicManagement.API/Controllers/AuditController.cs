using ClinicManagement.API.Models;
using ClinicManagement.Application.Features.Auth.Queries;
using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

/// <summary>
/// SuperAdmin-only audit log endpoint.
/// Provides full cross-clinic audit trail for debugging and compliance.
/// </summary>
[Authorize(Roles = "SuperAdmin")]
[Route("api/audit")]
public class AuditController : BaseApiController
{
    /// <summary>
    /// Query audit logs across all clinics.
    /// Filter by clinic, user, entity type, action, or date range.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(AuditLogsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] Guid? clinicId = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] string? entityType = null,
        [FromQuery] string? entityId = null,
        [FromQuery] AuditAction? action = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string? userSearch = null,
        [FromQuery] string? clinicSearch = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAuditLogsQuery(clinicId, userId, entityType, entityId, action, from, to, userSearch, clinicSearch, pageNumber, pageSize);
        var result = await Sender.Send(query, cancellationToken);
        return HandleResult(result, "Failed to retrieve audit logs");
    }
}
