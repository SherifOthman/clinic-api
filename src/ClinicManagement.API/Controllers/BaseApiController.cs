using ClinicManagement.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    protected readonly ISender Sender;
    protected readonly ILogger Logger;

    protected BaseApiController(ISender sender, ILogger logger)
    {
        Sender = sender;
        Logger = logger;
    }

    /// <summary>
    /// Create BadRequest response from error code and message
    /// </summary>
    protected IActionResult Error(string errorCode, string errorMessage, string title)
    {
        return BadRequest(new ApiProblemDetails
        {
            Code = errorCode,
            Title = title,
            Status = StatusCodes.Status400BadRequest,
            Detail = errorMessage
        });
    }
}
