using ClinicManagement.API.Models;
using ClinicManagement.Application.Staff.Commands;
using ClinicManagement.Application.Staff.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Authorize(Policy = "RequireClinic")]
[Route("api/staff")]
public class StaffController : BaseApiController
{
    private readonly IMediator _mediator;

    public StaffController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Invite a new staff member to the clinic
    /// </summary>
    [HttpPost("invite")]
    [ProducesResponseType(typeof(InviteStaffResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> InviteStaff([FromBody] InviteStaffRequest request, CancellationToken cancellationToken)
    {
        var command = new InviteStaffCommand(request.Role, request.Email, request.SpecializationId);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result.IsSuccess)
            return HandleResult(result, "Failed to send invitation");
        
        return Ok(result.Value);
    }

    /// <summary>
    /// Get all pending staff invitations for the clinic
    /// </summary>
    [HttpGet("invitations")]
    [ProducesResponseType(typeof(IEnumerable<PendingInvitationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPendingInvitations(CancellationToken cancellationToken)
    {
        var query = new GetPendingInvitationsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }


    /// <summary>
    /// Accept a staff invitation and register a new user account
    /// </summary>
    [HttpPost("invitations/{token}/accept-with-registration")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AcceptInvitationWithRegistration(
        string token,
        [FromBody] AcceptInvitationWithRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AcceptInvitationWithRegistrationCommand(
            token,
            request.FirstName,
            request.LastName,
            request.UserName,
            request.Password,
            request.PhoneNumber
        );
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result.IsSuccess)
            return HandleResult(result, "Invitation Failed");
        
        return NoContent();
    }

    /// <summary>
    /// Cancel a pending staff invitation
    /// </summary>
    [HttpDelete("invitations/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CancelInvitation(Guid id, CancellationToken cancellationToken)
    {
        var command = new CancelInvitationCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result.IsSuccess)
            return HandleResult(result, "Failed to cancel invitation");
        
        return NoContent();
    }

    /// <summary>
    /// Get list of all staff members in the clinic
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<StaffDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStaffList([FromQuery] string? role, CancellationToken cancellationToken)
    {
        var query = new GetStaffListQuery(role);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}

public record InviteStaffRequest(string Role, string Email, Guid? SpecializationId = null);
public record AcceptInvitationWithRegistrationRequest(
    string FirstName,
    string LastName,
    string UserName,
    string Password,
    string PhoneNumber
);
