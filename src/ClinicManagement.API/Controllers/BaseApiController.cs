using ClinicManagement.API.Extensions;
using ClinicManagement.API.Models;
using ClinicManagement.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Handles a Result<T> and returns appropriate HTTP response
    /// </summary>
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.Success)
            return Ok(result.Value);

        return BadRequest(result.ToApiError());
    }

    /// <summary>
    /// Handles a Result (no value) and returns appropriate HTTP response
    /// </summary>
    protected IActionResult HandleResult(Result result)
    {
        if (result.Success)
            return Ok();

        return BadRequest(result.ToApiError());
    }

    /// <summary>
    /// Handles a Result<T> for create operations and returns 201 Created
    /// </summary>
    protected IActionResult HandleCreateResult<T>(Result<T> result, string actionName, object? routeValues = null)
    {
        if (result.Success)
            return CreatedAtAction(actionName, routeValues, result.Value);

        return BadRequest(result.ToApiError());
    }

    /// <summary>
    /// Handles a Result for delete operations and returns 204 No Content
    /// </summary>
    protected IActionResult HandleDeleteResult(Result result)
    {
        if (result.Success)
            return NoContent();

        return BadRequest(result.ToApiError());
    }
}
