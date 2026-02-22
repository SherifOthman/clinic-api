using ClinicManagement.Application.Onboarding.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/onboarding")]
[Authorize]
public class OnboardingController : BaseApiController
{
    [HttpPost("complete")]
    public async Task<IActionResult> CompleteOnboarding([FromBody] CompleteOnboarding command)
    {
        var result = await Sender.Send(command);
        return HandleResult(result, "Onboarding Failed");
    }
}
