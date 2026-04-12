using ClinicManagement.API.Contracts.Onboarding;
using ClinicManagement.API.Models;
using ClinicManagement.Application.Features.Onboarding.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Authorize]
[Route("api/onboarding")]
public class OnboardingController : BaseApiController
{
    [HttpPost("complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CompleteOnboarding(
        [FromBody] CompleteOnboardingRequest request, CancellationToken cancellationToken)
    {
        var command = new CompleteOnboarding(
            request.ClinicName,
            request.SubscriptionPlanId,
            request.BranchName,
            request.AddressLine,
            request.CityNameEn,
            request.CityNameAr,
            request.StateNameEn,
            request.StateNameAr,
            request.ProvideMedicalServices,
            request.SpecializationId);

        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result, "Onboarding Failed");
    }
}
