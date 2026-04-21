using ClinicManagement.API.Models;
using ClinicManagement.API.RateLimiting;
using ClinicManagement.Application.Features.Reference.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicManagement.API.Controllers;

[Authorize]
[Route("api/chronic-diseases")]
[EnableRateLimiting(RateLimitPolicies.UserReads)]
public class ChronicDiseasesController : BaseApiController
{
    /// <summary>
    /// Get all chronic diseases (cached for 24 hours)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ChronicDiseaseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        var query = new GetChronicDiseasesQuery();
        var result = await Sender.Send(query, cancellationToken);
        return HandleResult(result, "Failed to retrieve chronic diseases");
    }
}
