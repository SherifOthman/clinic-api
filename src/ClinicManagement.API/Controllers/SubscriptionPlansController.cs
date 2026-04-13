using ClinicManagement.API.RateLimiting;
using ClinicManagement.Application.Features.SubscriptionPlans.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicManagement.API.Controllers;

[Route("api/subscription-plans")]
[EnableRateLimiting(RateLimitPolicies.AnonStatic)]
public class SubscriptionPlansController : BaseApiController
{
    /// <summary>
    /// Get all subscription plans
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<SubscriptionPlanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubscriptionPlans(CancellationToken ct)
    {
        var plans = await Sender.Send(new GetSubscriptionPlansQuery(), ct);
        return Ok(plans);
    }
}
