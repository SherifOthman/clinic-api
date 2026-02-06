using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.StaffInvitations.Commands.AcceptInvitation;
using ClinicManagement.Application.Features.StaffInvitations.Commands.InviteStaff;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StaffInvitationsController : BaseApiController
{
    private readonly IMediator _mediator;

    public StaffInvitationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Invite staff member (Doctor or Receptionist) to join clinic
    /// Only clinic owners can invite staff
    /// </summary>
    [HttpPost("invite")]
    [Authorize]
    public async Task<IActionResult> InviteStaff([FromBody] InviteStaffDto dto)
    {
        var command = new InviteStaffCommand(dto);
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Accept staff invitation and create account
    /// Public endpoint - no authentication required
    /// </summary>
    [HttpPost("accept")]
    [AllowAnonymous]
    public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInvitationDto dto)
    {
        var command = new AcceptInvitationCommand(dto);
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }
}
