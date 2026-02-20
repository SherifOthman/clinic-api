using ClinicManagement.Application.Staff.Commands;
using ClinicManagement.Application.Staff.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Authorize]
[Route("api/staff")]
public class StaffController : BaseApiController
{
    private readonly IMediator _mediator;

    public StaffController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("invite")]
    public async Task<IActionResult> InviteStaff([FromBody] InviteStaffRequest request, CancellationToken cancellationToken)
    {
        var command = new InviteStaffCommand(request.Role, request.Email);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet("invitations")]
    public async Task<IActionResult> GetPendingInvitations(CancellationToken cancellationToken)
    {
        var query = new GetPendingInvitationsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("invitations/{token}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetInvitationByToken(string token, CancellationToken cancellationToken)
    {
        var query = new GetInvitationByTokenQuery(token);
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result == null)
            return NotFound(new { message = "Invitation not found" });
        
        return Ok(result);
    }

    [HttpPost("invitations/{token}/accept")]
    public async Task<IActionResult> AcceptInvitation(string token, [FromBody] AcceptInvitationRequest request, CancellationToken cancellationToken)
    {
        var command = new AcceptInvitationCommand(token, request.UserId);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result.Success)
            return BadRequest(new { message = result.Message });
        
        return Ok(result);
    }

    [HttpPost("invitations/{token}/accept-with-registration")]
    [AllowAnonymous]
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
            request.Email,
            request.Password,
            request.PhoneNumber
        );
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result.Success)
            return BadRequest(new { message = result.Message });
        
        return Ok(result);
    }

    [HttpDelete("invitations/{id}")]
    public async Task<IActionResult> CancelInvitation(int id, CancellationToken cancellationToken)
    {
        // TODO: Implement cancel invitation command
        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> GetStaffList([FromQuery] string? role, CancellationToken cancellationToken)
    {
        var query = new GetStaffListQuery(role);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}

public record InviteStaffRequest(string Role, string Email);
public record AcceptInvitationRequest(int UserId);
public record AcceptInvitationWithRegistrationRequest(
    string FirstName,
    string LastName,
    string UserName,
    string Email,
    string Password,
    string PhoneNumber
);
