using ClinicManagement.API.Models;
using ClinicManagement.Application.Features.Dashboard.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/dashboard")]
public class DashboardController : BaseApiController
{
    /// <summary>Clinic staff dashboard stats — patients, staff, invitations, subscription.</summary>
    [HttpGet("stats")]
    [Authorize(Policy = "RequireClinic")]
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

    /// <summary>Recent patients for ClinicOwner dashboard.</summary>
    [HttpGet("recent-patients")]
    [Authorize(Policy = "RequireClinicOwner")]
    [ProducesResponseType(typeof(List<RecentPatientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecentPatients(CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetRecentPatientsQuery(5), cancellationToken);
        return HandleResult(result, "Failed to retrieve recent patients");
    }
}
