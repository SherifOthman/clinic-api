using ClinicManagement.API.Models;
using ClinicManagement.API.RateLimiting;
using ClinicManagement.Application.Features.Permissions.Commands;
using ClinicManagement.Application.Features.Permissions.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicManagement.API.Controllers;

[Authorize(Policy = "SuperAdmin")]
[Route("api/permissions")]
[EnableRateLimiting(RateLimitPolicies.UserReads)]
public class PermissionsController : BaseApiController
{
    /// <summary>
    /// Get all available permissions and current defaults per role.
    /// </summary>
    [HttpGet("role-defaults")]
    [ProducesResponseType(typeof(Dictionary<string, List<string>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoleDefaults(CancellationToken ct)
    {
        var result = await Sender.Send(new GetRoleDefaultPermissionsQuery(), ct);
        return HandleResult(result, "Failed to retrieve role defaults");
    }

    /// <summary>
    /// Get all available permission values.
    /// </summary>
    [HttpGet("available")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public IActionResult GetAvailablePermissions()
    {
        var permissions = Enum.GetNames<Domain.Enums.Permission>().ToList();
        return Ok(permissions);
    }

    /// <summary>
    /// Update default permissions for a role (Doctor, Receptionist, Owner).
    /// Affects new staff members — existing members keep their current permissions.
    /// </summary>
    [HttpPut("role-defaults/{role}")]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetRoleDefaults(
        [FromRoute] string role,
        [FromBody] List<string> permissions,
        CancellationToken ct)
    {
        var command = new SetRoleDefaultPermissionsCommand(role, permissions);
        var result  = await Sender.Send(command, ct);
        return HandleNoContent(result, "Failed to update role defaults");
    }
}
