using ClinicManagement.API.Models;
using ClinicManagement.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _sender;
    protected ISender Sender => _sender ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected IActionResult HandleResult(Result result, string title)
    {
        if (result.IsSuccess)
            return Ok();

        return BadRequest(new ApiProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Title = title,
            Status = StatusCodes.Status400BadRequest,
            Detail = result.ErrorMessage,
            Code = result.ErrorCode,
            Errors = result.ValidationErrors,
            TraceId = HttpContext.TraceIdentifier
        });
    }

    protected IActionResult HandleResult<T>(Result<T> result, string title)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(new ApiProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Title = title,
            Status = StatusCodes.Status400BadRequest,
            Detail = result.ErrorMessage,
            Code = result.ErrorCode,
            Errors = result.ValidationErrors,
            TraceId = HttpContext.TraceIdentifier
        });
    }
}
