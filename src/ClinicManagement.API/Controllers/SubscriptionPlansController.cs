using ClinicManagement.Application.SubscriptionPlans.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/subscription-plans")]
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
