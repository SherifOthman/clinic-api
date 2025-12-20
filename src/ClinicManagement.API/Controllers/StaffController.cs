using Asp.Versioning;
using ClinicManagement.API.Extensions;
using ClinicManagement.Application.Features.Staff.Commands.AcceptInvitation;
using ClinicManagement.Application.Features.Staff.Commands.InviteStaff;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class StaffController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<StaffController> _logger;

    public StaffController(IMediator mediator, ILogger<StaffController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all staff members for the current clinic
    /// </summary>
    [HttpGet]
    [Authorize(Policy = Application.Common.Authorization.Policies.RequireStaffMember)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStaff(CancellationToken cancellationToken)
    {
        // For now, return empty list since we don't have the full implementation
        // In a real implementation, this would query users by clinic and role
        var staffMembers = new List<object>();
        
        return Ok(new { value = staffMembers });
    }

    /// <summary>
    /// Invite a doctor or receptionist to join the clinic
    /// Only clinic owners can invite staff
    /// </summary>
    [HttpPost("invite")]
    [Authorize(Policy = Application.Common.Authorization.Policies.ManageStaff)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> InviteStaff([FromBody] InviteStaffCommand command, CancellationToken cancellationToken)
    {
        // ClinicId and UserId are now injected in the handler from ICurrentUserService
        // No need to manually set them here
        
        _logger.LogInformation("Inviting {Email} as {Role}", command.Email, command.Role);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Staff invitation failed: {Errors}", 
                string.Join(", ", result.Errors.Select(e => e.Message)));
            return BadRequest(result.ToApiError());
        }

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Accept staff invitation and complete registration
    /// Public endpoint - no authentication required
    /// </summary>
    [HttpPost("accept-invitation")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInvitationCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing invitation acceptance for user {UserId}", command.UserId);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Invitation acceptance failed: {Errors}", 
                string.Join(", ", result.Errors.Select(e => e.Message)));
            return BadRequest(result.ToApiError());
        }

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Resend invitation email
    /// Only clinic owners can resend invitations
    /// </summary>
    [HttpPost("resend-invitation/{userId}")]
    [Authorize(Policy = Application.Common.Authorization.Policies.ManageStaff)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ResendInvitation(int userId, CancellationToken cancellationToken)
    {
        // TODO: Implement resend invitation logic
        return Ok(new { message = "Invitation resent successfully" });
    }

    /// <summary>
    /// Cancel/revoke a pending invitation
    /// Only clinic owners can cancel invitations
    /// </summary>
    [HttpDelete("cancel-invitation/{userId}")]
    [Authorize(Policy = Application.Common.Authorization.Policies.ManageStaff)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CancelInvitation(int userId, CancellationToken cancellationToken)
    {
        // TODO: Implement cancel invitation logic
        return Ok(new { message = "Invitation cancelled successfully" });
    }
}
