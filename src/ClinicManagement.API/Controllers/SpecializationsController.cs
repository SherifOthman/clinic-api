using ClinicManagement.API.RateLimiting;
using ClinicManagement.Application.Features.Reference.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicManagement.API.Controllers;

[Route("api/specializations")]
[EnableRateLimiting(RateLimitPolicies.AnonStatic)]
public class SpecializationsController : BaseApiController
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<SpecializationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await Sender.Send(new GetSpecializationsQuery(), ct);
        return Ok(result);
    }
}
