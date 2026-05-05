using ClinicManagement.API.RateLimiting;
using ClinicManagement.Application.Features.Reference.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicManagement.API.Controllers;

[Route("api/specializations")]
public class SpecializationsController : BaseApiController
{
    [HttpGet]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.AnonStatic)]
    [ProducesResponseType(typeof(List<SpecializationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await Sender.Send(new GetSpecializationsQuery(), ct);
        return HandleResult(result, "Failed to retrieve specializations");
    }
}
