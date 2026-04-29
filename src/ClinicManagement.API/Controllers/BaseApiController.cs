using ClinicManagement.API.Models;
using ClinicManagement.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    // Injected via property injection by ASP.NET DI — avoids constructor boilerplate
    // in every derived controller while keeping the dependency explicit.
    [FromServices]
    public ISender Sender { get; set; } = null!;

    protected IActionResult HandleResult(Result result, string title)
    {
        if (result.IsSuccess) return Ok();
        return Problem(result, title);
    }

    protected IActionResult HandleResult<T>(Result<T> result, string title)
    {
        if (result.IsSuccess) return Ok(result.Value);
        return Problem(result, title);
    }

    protected IActionResult HandleNoContent(Result result, string title)
    {
        if (result.IsSuccess) return NoContent();
        return Problem(result, title);
    }

    private BadRequestObjectResult Problem(Result result, string title) =>
        BadRequest(new ApiProblemDetails
        {
            Type    = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Title   = title,
            Status  = StatusCodes.Status400BadRequest,
            Detail  = result.ErrorMessage,
            Code    = result.ErrorCode,
            Errors  = result.ValidationErrors,
            TraceId = HttpContext.TraceIdentifier,
        });
}
