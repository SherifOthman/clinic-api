using ClinicManagement.Application.Features.Auth.Commands.CompleteOnboarding;
using ClinicManagement.Application.Features.SubscriptionPlans.Queries.GetSubscriptionPlans;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mapster;

namespace ClinicManagement.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class OnboardingController : BaseApiController
{
    public OnboardingController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet("subscription-plans")]
    public async Task<IActionResult> GetSubscriptionPlans()
    {
        var result = await Mediator.Send(new GetSubscriptionPlansQuery());
        return Ok(result);
    }

    [HttpPost("complete")]
    public async Task<IActionResult> CompleteOnboarding([FromBody] CompleteOnboardingRequest request)
    {
        var command = new CompleteOnboardingCommand(
            request.ClinicName, 
            request.SubscriptionPlanId,
            request.BranchName,
            request.BranchAddress,
            request.CountryId,
            request.StateId,
            request.CityId,
            request.BranchPhoneNumbers.Adapt<List<BranchPhoneNumberDto>>()
        );
        var result = await Mediator.Send(command);
        
        if (result.Success)
            return Ok(new { ClinicId = result.Value });
        
        // Return detailed validation errors for frontend mapping
        if (result.Errors?.Any() == true)
        {
            return BadRequest(new { 
                message = result.Message,
                errors = result.Errors.Select(e => new { field = e.Field, message = e.Message })
            });
        }
        
        return BadRequest(new { message = result.Message });
    }
}

public record CompleteOnboardingRequest(
    string ClinicName, 
    int SubscriptionPlanId,
    string BranchName,
    string BranchAddress,
    int CountryId,
    int? StateId,
    int CityId,
    List<BranchPhoneNumberRequest> BranchPhoneNumbers
);

public record BranchPhoneNumberRequest(
    string PhoneNumber,
    string? Label = null
);