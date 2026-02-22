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
        
        if (!result.IsSuccess)
            return HandleResult(result, "Failed to send invitation");
        
        return Ok(result.Value);
    }

    [HttpGet("invitations")]
    public async Task<IActionResult> GetPendingInvitations(CancellationToken cancellationToken)
    {
        var query = new GetPendingInvitationsQuery();
        var result = await _mediator.Send(query, cancellationToken);
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
            request.Password,
            request.PhoneNumber
        );
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result.IsSuccess)
            return HandleResult(result,"Invitaiton Filed");
        
        return NoContent();
    }

    [HttpDelete("invitations/{id}")]
    public async Task<IActionResult> CancelInvitation(int id, CancellationToken cancellationToken)
    {
        var command = new CancelInvitationCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result.IsSuccess)
            return HandleResult(result, "Failed to cancel invitation");
        
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
public record AcceptInvitationWithRegistrationRequest(
    string FirstName,
    string LastName,
    string UserName,
    string Password,
    string PhoneNumber
);
