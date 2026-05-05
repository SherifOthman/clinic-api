using ClinicManagement.API.Authorization;
using ClinicManagement.API.RateLimiting;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Features.Reference.Commands;
using ClinicManagement.Application.Features.Reference.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicManagement.API.Controllers.Admin;

/// <summary>SuperAdmin — full CRUD for specializations.</summary>
[Route("api/admin/specializations")]
[Authorize(Policy = AuthorizationPolicies.SuperAdmin)]
public class AdminSpecializationsController : BaseApiController
{
    /// <summary>Paginated list — includes inactive items.</summary>
    [HttpGet]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
    [ProducesResponseType(typeof(PaginatedResult<SpecializationAdminDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaginated(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize   = 10,
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetSpecializationsPaginatedQuery(pageNumber, pageSize), ct);
        return HandleResult(result, "Failed to retrieve specializations");
    }

    /// <summary>Create a new specialization.</summary>
    [HttpPost]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSpecializationCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        return HandleResult(result, "Failed to create specialization");
    }

    /// <summary>Update an existing specialization.</summary>
    [HttpPut("{id:guid}")]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSpecializationCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command with { Id = id }, ct);
        return HandleNoContent(result, "Failed to update specialization");
    }

    /// <summary>Deactivate a specialization (soft-delete — preserves existing doctor assignments).</summary>
    [HttpDelete("{id:guid}")]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new DeleteSpecializationCommand(id), ct);
        return HandleNoContent(result, "Failed to delete specialization");
    }
}
