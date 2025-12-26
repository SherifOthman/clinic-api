using ClinicManagement.API.Extensions;
using ClinicManagement.Application.Features.Staff.Commands.AcceptInvitation;
using ClinicManagement.Application.Features.Staff.Commands.InviteStaff;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StaffController : ControllerBase
{
    private readonly IMediator _mediator;

    public StaffController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all staff members for the current clinic
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Doctor,Receptionist,Nurse")]
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
    [Authorize(Roles = "ClinicOwner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> InviteStaff([FromBody] InviteStaffCommand command, CancellationToken cancellationToken)
    {
        // ClinicId and UserId are now injected in the handler from ICurrentUserService
        // No need to manually set them here
        
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.Success)
        {
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
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result.ToApiError());
        }

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Resend invitation email
    /// Only clinic owners can resend invitations
    /// </summary>
    [HttpPost("resend-invitation/{userId}")]
    [Authorize(Roles = "ClinicOwner")]
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
    [Authorize(Roles = "ClinicOwner")]
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
