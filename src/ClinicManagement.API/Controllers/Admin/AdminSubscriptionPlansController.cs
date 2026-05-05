using ClinicManagement.API.Authorization;
using ClinicManagement.API.RateLimiting;
using ClinicManagement.Application.Features.SubscriptionPlans.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicManagement.API.Controllers.Admin;

/// <summary>SuperAdmin — full CRUD for subscription plans.</summary>
[Route("api/admin/subscription-plans")]
[Authorize(Policy = AuthorizationPolicies.SuperAdmin)]
[EnableRateLimiting(RateLimitPolicies.UserWrites)]
public class AdminSubscriptionPlansController : BaseApiController
{
    /// <summary>Create a new subscription plan.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSubscriptionPlanCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        return HandleResult(result, "Failed to create subscription plan");
    }

    /// <summary>Update an existing subscription plan.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSubscriptionPlanCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command with { Id = id }, ct);
        return HandleNoContent(result, "Failed to update subscription plan");
    }

    /// <summary>Toggle a plan's active status (active ↔ inactive).</summary>
    [HttpPatch("{id:guid}/toggle")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Toggle(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new ToggleSubscriptionPlanCommand(id), ct);
        return HandleNoContent(result, "Failed to toggle subscription plan");
    }
}
