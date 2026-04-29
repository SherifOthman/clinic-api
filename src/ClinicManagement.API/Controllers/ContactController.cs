using ClinicManagement.API.RateLimiting;
using ClinicManagement.Application.Features.Contact.Commands;
using ClinicManagement.Application.Features.Contact.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicManagement.API.Controllers;

[Route("api/contact")]
public class ContactController : BaseApiController
{
    /// <summary>Public — submit a contact form message.</summary>
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.AnonContact)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Send([FromBody] SendContactMessageCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        return HandleNoContent(result, "Failed to send message");
    }

    /// <summary>SuperAdmin only — view all contact messages.</summary>
    [HttpGet]
    [Authorize(Policy = "SuperAdmin")]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
    [ProducesResponseType(typeof(List<ContactMessageDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMessages([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetContactMessagesQuery(page, pageSize), ct);
        return Ok(result);
    }
}
