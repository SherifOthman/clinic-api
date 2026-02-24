using ClinicManagement.API.Models;
using ClinicManagement.Application.Onboarding.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Authorize] // Only requires authentication, not clinic membership (user is onboarding)
[Route("api/onboarding")]
public class OnboardingController : BaseApiController
{
    /// <summary>
    /// Complete the onboarding process by creating a clinic
    /// </summary>
    [HttpPost("complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CompleteOnboarding([FromBody] CompleteOnboarding command)
    {
        var result = await Sender.Send(command);
        return HandleResult(result, "Onboarding Failed");
    }
}
