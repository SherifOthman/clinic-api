using ClinicManagement.API.Models;
using ClinicManagement.API.RateLimiting;
using ClinicManagement.Application.Features.Onboarding.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicManagement.API.Controllers;

[Authorize]
[Route("api/onboarding")]
public class OnboardingController : BaseApiController
{
    [HttpPost("complete")]
    [EnableRateLimiting(RateLimitPolicies.UserOnce)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CompleteOnboarding(
        [FromBody] CompleteOnboarding request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(request, cancellationToken);
        return HandleResult(result, "Onboarding Failed");
    }
}
