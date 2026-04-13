using ClinicManagement.API.Models;
using ClinicManagement.API.RateLimiting;
using ClinicManagement.Application.Features.Branches.Commands;
using ClinicManagement.Application.Features.Branches.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicManagement.API.Controllers;

[Authorize(Policy = "RequireClinicOwner")]
[Route("api/branches")]
public class BranchesController : BaseApiController
{
    [HttpGet]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
    [ProducesResponseType(typeof(List<BranchDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBranches(CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetBranchesQuery(), cancellationToken);
        return HandleResult(result, "Failed to retrieve branches");
    }

    [HttpPost]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBranch([FromBody] CreateBranchCommand command, CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : HandleResult(result, "Failed to create branch");
    }

    [HttpPut("{id:guid}")]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBranch([FromRoute] Guid id, [FromBody] UpdateBranchCommand command, CancellationToken cancellationToken = default)
    {
        var merged = command with { Id = id };
        return HandleNoContent(await Sender.Send(merged, cancellationToken), "Failed to update branch");
    }

    [HttpPatch("{id:guid}/active-status")]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetActiveStatus([FromRoute] Guid id, [FromBody] SetBranchActiveStatusCommand command, CancellationToken cancellationToken = default)
    {
        var merged = command with { Id = id };
        return HandleNoContent(await Sender.Send(merged, cancellationToken), "Failed to update branch status");
    }
}
