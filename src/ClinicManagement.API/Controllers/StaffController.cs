using ClinicManagement.API.Contracts.Staff;
using ClinicManagement.API.Models;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Staff.Commands;
using ClinicManagement.Application.Staff.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Authorize(Policy = "RequireClinicOwner")]
[Route("api/staff")]
public class StaffController(IMediator mediator) : BaseApiController
{
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
        var result = await mediator.Send(command, cancellationToken);
        
        return !result.IsSuccess ? HandleResult(result, "Failed to send invitation") : Ok(result.Value);
    }

    /// <summary>
    /// Get paginated list of invitations, optionally filtered by status
    /// </summary>
    [HttpGet("invitations")]
    [ProducesResponseType(typeof(PaginatedResult<InvitationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetInvitations(
        [FromQuery] InvitationStatus? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetInvitationsQuery(status, pageNumber, pageSize);
        var result = await mediator.Send(query, cancellationToken);
        return HandleResult(result, "Failed to retrieve invitations");
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
        var result = await mediator.Send(command, cancellationToken);

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
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return HandleResult(result, "Failed to cancel invitation");

        return NoContent();
    }

    /// <summary>
    /// Get paginated list of staff members in the clinic, optionally filtered by role
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<StaffDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStaffList(
        [FromQuery] string? role,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStaffListQuery(role, pageNumber, pageSize);
        var result = await mediator.Send(query, cancellationToken);
        return HandleResult(result, "Failed to retrieve staff");
    }

    /// <summary>
    /// Set clinic owner as a doctor (can be done after onboarding)
    /// </summary>
    [HttpPost("set-owner-as-doctor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SetOwnerAsDoctor([FromBody] SetOwnerAsDoctorRequest request, CancellationToken cancellationToken)
    {
        var command = new SetOwnerAsDoctor(
            request.SpecializationId,
            request.LicenseNumber,
            request.YearsOfExperience
        );
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return HandleResult(result, "Failed to set owner as doctor");

        return NoContent();
    }
}
