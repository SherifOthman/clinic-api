using ClinicManagement.API.Models;
using ClinicManagement.API.RateLimiting;
using ClinicManagement.Application.Features.ClinicSettings.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicManagement.API.Controllers;

[Route("api/clinic")]
[Authorize(Policy = "RequireClinicOwner")]
public class ClinicController : BaseApiController
{
    /// <summary>Update clinic-level settings (week start day, etc.).</summary>
    [HttpPatch("settings")]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSettings(
        [FromBody] UpdateClinicSettingsRequest request,
        CancellationToken ct)
    {
        var result = await Sender.Send(new UpdateClinicSettingsCommand(request.WeekStartDay), ct);
        return HandleNoContent(result, "Failed to update clinic settings");
    }
}

/// <summary>Request body for PATCH /api/clinic/settings</summary>
public record UpdateClinicSettingsRequest(int WeekStartDay);
