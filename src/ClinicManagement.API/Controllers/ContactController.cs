using ClinicManagement.API.Authorization;
using ClinicManagement.API.Models;
using ClinicManagement.API.RateLimiting;
using ClinicManagement.Application.Common.Models;
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
    [Authorize(Policy = AuthorizationPolicies.SuperAdmin)]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
    [ProducesResponseType(typeof(PaginatedResult<ContactMessageDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMessages([FromQuery] PaginationRequest pagination, CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetContactMessagesQuery(pagination.PageNumber, pagination.PageSize), ct);
        return HandleResult(result, "Failed to retrieve contact messages");
    }

    /// <summary>SuperAdmin only — count of unread contact messages (for sidebar badge).</summary>
    [HttpGet("unread-count")]
    [Authorize(Policy = AuthorizationPolicies.SuperAdmin)]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetContactMessagesUnreadCountQuery(), ct);
        return HandleResult(result, "Failed to retrieve unread count");
    }
}
