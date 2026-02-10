using ClinicManagement.API.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Onboarding.Commands.CompleteOnboarding;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class OnboardingController : BaseApiController
{
    private readonly IMediator _mediator;

    public OnboardingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteOnboarding(CompleteOnboardingDto dto, CancellationToken cancellationToken)
    {
        var command = new CompleteOnboardingCommand(dto);
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
