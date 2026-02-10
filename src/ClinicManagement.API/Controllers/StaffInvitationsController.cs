using ClinicManagement.API.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.StaffInvitations.Commands.AcceptInvitation;
using ClinicManagement.Application.Features.StaffInvitations.Commands.InviteStaff;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/staff-invitations")]
public class StaffInvitationsController : BaseApiController
{
    private readonly IMediator _mediator;

    public StaffInvitationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("invite")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InviteStaff(InviteStaffDto dto, CancellationToken cancellationToken)
    {
        var command = new InviteStaffCommand(dto);
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("accept")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AcceptInvitation(AcceptInvitationDto dto, CancellationToken cancellationToken)
    {
        var command = new AcceptInvitationCommand(dto);
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
