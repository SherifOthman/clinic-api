using ClinicManagement.API.RateLimiting;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Features.Notifications.Commands.MarkRead;
using ClinicManagement.Application.Features.Notifications.Queries.GetNotifications;
using ClinicManagement.Application.Features.Notifications.Queries.GetUnreadCount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicManagement.API.Controllers;

[Route("api/notifications")]
[Authorize]
public class NotificationsController : BaseApiController
{
    /// <summary>Paginated list of notifications for the current user.</summary>
    [HttpGet]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
    [ProducesResponseType(typeof(PaginatedResult<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize   = 20,
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetNotificationsQuery(pageNumber, pageSize), ct);
        return HandleResult(result, "Failed to retrieve notifications");
    }

    /// <summary>Count of unread notifications — used by the bell badge.</summary>
    [HttpGet("unread-count")]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetUnreadCountQuery(), ct);
        return HandleResult(result, "Failed to retrieve unread count");
    }

    /// <summary>Mark a single notification as read.</summary>
    [HttpPatch("{id:guid}/read")]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct = default)
    {
        var result = await Sender.Send(new MarkNotificationReadCommand(id), ct);
        return HandleNoContent(result, "Failed to mark notification as read");
    }

    /// <summary>Mark all notifications as read.</summary>
    [HttpPatch("read-all")]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct = default)
    {
        var result = await Sender.Send(new MarkNotificationReadCommand(null), ct);
        return HandleNoContent(result, "Failed to mark all notifications as read");
    }
}
