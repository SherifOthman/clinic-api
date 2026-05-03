using ClinicManagement.API.Authorization;
using ClinicManagement.API.Models;
using ClinicManagement.API.RateLimiting;
using ClinicManagement.Application.Features.Dashboard.Queries;
using ClinicManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicManagement.API.Controllers;

[Route("api/dashboard")]
[EnableRateLimiting(RateLimitPolicies.UserReads)]
public class DashboardController : BaseApiController
{
    /// <summary>Public aggregate stats for the marketing website — no auth required.</summary>
    [HttpGet("stats/public")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.AnonStatic)]
    [ProducesResponseType(typeof(PublicStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublicStats(CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetPublicStatsQuery(), cancellationToken);
        return HandleResult(result, "Failed to retrieve public stats");
    }

    /// <summary>Clinic staff dashboard stats — patients, staff, invitations, subscription.</summary>
    [HttpGet("stats")]
    [Authorize]
    [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetDashboardStatsQuery(), cancellationToken);
        return HandleResult(result, "Failed to retrieve dashboard stats");
    }

    /// <summary>SuperAdmin dashboard stats — cross-clinic totals.</summary>
    [HttpGet("stats/superadmin")]
    [Authorize(Policy = "SuperAdmin")]
    [ProducesResponseType(typeof(SuperAdminStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSuperAdminStats(CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetSuperAdminStatsQuery(), cancellationToken);
        return HandleResult(result, "Failed to retrieve superadmin stats");
    }

    /// <summary>Recent patients for any staff member with patient access.</summary>
    [HttpGet("recent-patients")]
    [RequirePermission(Permission.ViewPatients)]
    [ProducesResponseType(typeof(List<RecentPatientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecentPatients(CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetRecentPatientsQuery(5), cancellationToken);
        return HandleResult(result, "Failed to retrieve recent patients");
    }
}
