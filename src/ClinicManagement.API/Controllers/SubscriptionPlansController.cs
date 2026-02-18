using ClinicManagement.Application.Features.SubscriptionPlans.Queries.GetSubscriptionPlans;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/subscription-plans")]
[Produces("application/json")]
public class SubscriptionPlansController : BaseApiController
{
    public SubscriptionPlansController(ISender sender, ILogger<SubscriptionPlansController> logger) 
        : base(sender, logger)
    {
    }

    /// <summary>
    /// Get all subscription plans
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [OutputCache(PolicyName = "ReferenceData")]
    [ProducesResponseType(typeof(List<SubscriptionPlanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubscriptionPlans(CancellationToken ct)
    {
        var plans = await Sender.Send(new GetSubscriptionPlansQuery(), ct);
        return Ok(plans);
    }
}
