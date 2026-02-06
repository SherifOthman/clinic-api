using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Onboarding.Commands.CompleteOnboarding;
using ClinicManagement.Application.Features.Onboarding.Queries.GetSubscriptionPlans;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OnboardingController : BaseApiController
{
    private readonly IMediator _mediator;

    public OnboardingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all active subscription plans
    /// </summary>
    [HttpGet("subscription-plans")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSubscriptionPlans()
    {
        var query = new GetSubscriptionPlansQuery();
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Complete onboarding for clinic owner
    /// </summary>
    [HttpPost("complete")]
    [Authorize]
    public async Task<IActionResult> CompleteOnboarding([FromBody] CompleteOnboardingDto dto)
    {
        var command = new CompleteOnboardingCommand(dto);
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }
}
