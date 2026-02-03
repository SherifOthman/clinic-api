using ClinicManagement.API.Extensions;
using ClinicManagement.API.Models;
using ClinicManagement.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
public abstract class BaseApiController : ControllerBase
{
    protected readonly IMediator Mediator;

    protected BaseApiController(IMediator mediator)
    {
        Mediator = mediator;
    }

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.Success)
            return Ok(result.Value);

        return BadRequest(result.ToApiError());
    }

    protected IActionResult HandleResult(Result result)
    {
        if (result.Success)
            return Ok();

        return BadRequest(result.ToApiError());
    }

    protected IActionResult HandleCreateResult<T>(Result<T> result, string actionName, object? routeValues = null)
    {
        if (result.Success)
            return CreatedAtAction(actionName, routeValues, result.Value);

        return BadRequest(result.ToApiError());
    }

    protected IActionResult HandleDeleteResult(Result result)
    {
        if (result.Success)
            return NoContent();

        return BadRequest(result.ToApiError());
    }
}
