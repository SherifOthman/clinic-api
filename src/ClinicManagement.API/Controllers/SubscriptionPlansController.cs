using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Onboarding.Queries.GetSubscriptionPlans;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/subscription-plans")]
[AllowAnonymous]
public class SubscriptionPlansController : BaseApiController
{
    private readonly IMediator _mediator;

    public SubscriptionPlansController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SubscriptionPlanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubscriptionPlans(CancellationToken cancellationToken)
    {
        var query = new GetSubscriptionPlansQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
}
