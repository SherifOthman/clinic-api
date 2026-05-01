using ClinicManagement.API.Models;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
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

    private ObjectResult Problem(Result result, string title)
    {
        var statusCode = MapErrorCodeToStatus(result.ErrorCode);

        var details = new ApiProblemDetails
        {
            Type    = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Title   = title,
            Status  = statusCode,
            Detail  = result.ErrorMessage,
            Code    = result.ErrorCode,
            Errors  = result.ValidationErrors,
            TraceId = HttpContext.TraceIdentifier,
        };

        return StatusCode(statusCode, details);
    }

    /// <summary>
    /// Maps domain error codes to the correct HTTP status code.
    /// Every error code must map to a semantically correct status — returning
    /// 400 for a NOT_FOUND is wrong and breaks client error handling.
    /// </summary>
    private static int MapErrorCodeToStatus(string? errorCode) => errorCode switch
    {
        // 401 Unauthorized — authentication required or credentials invalid
        ErrorCodes.INVALID_CREDENTIALS  => StatusCodes.Status401Unauthorized,
        ErrorCodes.TOKEN_INVALID        => StatusCodes.Status401Unauthorized,
        ErrorCodes.ACCOUNT_LOCKED       => StatusCodes.Status401Unauthorized,
        ErrorCodes.EMAIL_NOT_CONFIRMED  => StatusCodes.Status401Unauthorized,

        // 403 Forbidden — authenticated but not allowed
        ErrorCodes.FORBIDDEN            => StatusCodes.Status403Forbidden,
        ErrorCodes.STAFF_INACTIVE       => StatusCodes.Status403Forbidden,

        // 404 Not Found
        ErrorCodes.NOT_FOUND            => StatusCodes.Status404NotFound,
        ErrorCodes.USER_NOT_FOUND       => StatusCodes.Status404NotFound,
        ErrorCodes.PATIENT_NOT_FOUND    => StatusCodes.Status404NotFound,
        ErrorCodes.PLAN_NOT_FOUND       => StatusCodes.Status404NotFound,
        ErrorCodes.CLINIC_NOT_FOUND     => StatusCodes.Status404NotFound,

        // 409 Conflict — resource already exists or state conflict
        ErrorCodes.ALREADY_EXISTS           => StatusCodes.Status409Conflict,
        ErrorCodes.EMAIL_ALREADY_EXISTS     => StatusCodes.Status409Conflict,
        ErrorCodes.USERNAME_ALREADY_EXISTS  => StatusCodes.Status409Conflict,
        ErrorCodes.ALREADY_ONBOARDED        => StatusCodes.Status409Conflict,
        ErrorCodes.EMAIL_ALREADY_CONFIRMED  => StatusCodes.Status409Conflict,
        ErrorCodes.CONFLICT                 => StatusCodes.Status409Conflict,
        ErrorCodes.INVITATION_ALREADY_ACCEPTED => StatusCodes.Status409Conflict,
        ErrorCodes.INVITATION_ALREADY_CANCELED => StatusCodes.Status409Conflict,

        // 422 Unprocessable — request is well-formed but business rules reject it
        ErrorCodes.OPERATION_NOT_ALLOWED    => StatusCodes.Status422UnprocessableEntity,
        ErrorCodes.INVITATION_CANCELED      => StatusCodes.Status422UnprocessableEntity,
        ErrorCodes.INVITATION_EXPIRED       => StatusCodes.Status422UnprocessableEntity,
        ErrorCodes.PATIENT_NOT_DELETED      => StatusCodes.Status422UnprocessableEntity,

        // 400 Bad Request — validation errors and everything else
        _ => StatusCodes.Status400BadRequest,
    };
}
