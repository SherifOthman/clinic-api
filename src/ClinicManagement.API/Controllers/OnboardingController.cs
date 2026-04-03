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
    /// <summary>
    /// Complete the onboarding process by creating a clinic
    /// </summary>
    [HttpPost("complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CompleteOnboarding([FromBody] CompleteOnboardingRequest request)
    {
        var command = new CompleteOnboarding(
            request.ClinicName,
            request.SubscriptionPlanId,
            request.BranchName,
            request.AddressLine,
            request.CountryGeoNameId,
            request.StateGeoNameId,
            request.CityGeoNameId,
            request.ProvideMedicalServices,
            request.SpecializationId
        );
        
        var result = await Sender.Send(command);
        return HandleResult(result, "Onboarding Failed");
    }
}
