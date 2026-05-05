using ClinicManagement.API.RateLimiting;
using ClinicManagement.Application.Features.SubscriptionPlans.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicManagement.API.Controllers;

[Route("api/subscription-plans")]
public class SubscriptionPlansController : BaseApiController
{
    [HttpGet]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.AnonStatic)]
    [ProducesResponseType(typeof(List<SubscriptionPlanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await Sender.Send(new GetSubscriptionPlansQuery(), ct);
        return HandleResult(result, "Failed to retrieve subscription plans");
    }
}
