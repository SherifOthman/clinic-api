using ClinicManagement.Application.Onboarding.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Authorize] // Only requires authentication, not clinic membership (user is onboarding)
[Route("api/onboarding")]
public class OnboardingController : BaseApiController
{
    [HttpPost("complete")]
    public async Task<IActionResult> CompleteOnboarding([FromBody] CompleteOnboarding command)
    {
        var result = await Sender.Send(command);
        return HandleResult(result, "Onboarding Failed");
    }
}
