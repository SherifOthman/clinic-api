using ClinicManagement.API.Models;
using ClinicManagement.API.RateLimiting;
using ClinicManagement.Application.Features.Reference.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicManagement.API.Controllers;

[Route("api/chronic-diseases")]
public class ChronicDiseasesController : BaseApiController
{
    [HttpGet]
    [Authorize]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
    [ProducesResponseType(typeof(List<ChronicDiseaseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetChronicDiseasesQuery(), ct);
        return HandleResult(result, "Failed to retrieve chronic diseases");
    }
}
