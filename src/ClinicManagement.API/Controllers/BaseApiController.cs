using ClinicManagement.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _sender;
    protected ISender Sender => _sender ??= HttpContext.RequestServices.GetRequiredService<ISender>();

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
