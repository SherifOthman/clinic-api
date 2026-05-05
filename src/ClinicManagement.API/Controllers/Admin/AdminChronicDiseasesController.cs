using ClinicManagement.API.Authorization;
using ClinicManagement.API.RateLimiting;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Features.Reference.Commands;
using ClinicManagement.Application.Features.Reference.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicManagement.API.Controllers.Admin;

/// <summary>SuperAdmin — full CRUD for chronic diseases.</summary>
[Route("api/admin/chronic-diseases")]
[Authorize(Policy = AuthorizationPolicies.SuperAdmin)]
public class AdminChronicDiseasesController : BaseApiController
{
    /// <summary>Paginated list — includes inactive items.</summary>
    [HttpGet]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
    [ProducesResponseType(typeof(PaginatedResult<ChronicDiseaseAdminDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaginated(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize   = 10,
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetChronicDiseasesPaginatedQuery(pageNumber, pageSize), ct);
        return HandleResult(result, "Failed to retrieve chronic diseases");
    }

    /// <summary>Create a new chronic disease.</summary>
    [HttpPost]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateChronicDiseaseCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        return HandleResult(result, "Failed to create chronic disease");
    }

    /// <summary>Update an existing chronic disease.</summary>
    [HttpPut("{id:guid}")]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateChronicDiseaseCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command with { Id = id }, ct);
        return HandleNoContent(result, "Failed to update chronic disease");
    }

    /// <summary>Deactivate a chronic disease (soft-delete — preserves existing patient records).</summary>
    [HttpDelete("{id:guid}")]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new DeleteChronicDiseaseCommand(id), ct);
        return HandleNoContent(result, "Failed to delete chronic disease");
    }
}
