using ClinicManagement.API.Contracts.Staff;
using ClinicManagement.API.Models;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Features.Staff.Commands;
using ClinicManagement.Application.Features.Staff.Dtos;
using ClinicManagement.Application.Features.Staff.Queries;
using ClinicManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Authorize(Policy = "RequireClinicOwner")]
[Route("api/staff")]
public class StaffController : BaseApiController
{
    [HttpPost("invite")]
    [ProducesResponseType(typeof(InviteStaffResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InviteStaff(
        [FromBody] InviteStaffRequest request, CancellationToken cancellationToken)
    {
        var command = new InviteStaffCommand(request.Role, request.Email, request.SpecializationId);
        var result = await Sender.Send(command, cancellationToken);
        return !result.IsSuccess ? HandleResult(result, "Failed to send invitation") : Ok(result.Value);
    }

    [HttpGet("invitations")]
    [ProducesResponseType(typeof(PaginatedResult<InvitationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvitations(
        [FromQuery] InvitationStatus? status,
        [FromQuery] string? role,
        [FromQuery] SortedPaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        var query = new GetInvitationsQuery(status, role, pagination.SortBy, pagination.SortDirection, pagination.PageNumber, pagination.PageSize);
        var result = await Sender.Send(query, cancellationToken);
        return HandleResult(result, "Failed to retrieve invitations");
    }

    [HttpGet("invitations/{id:guid}")]
    [ProducesResponseType(typeof(InvitationDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvitationDetail(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetInvitationDetailQuery(id), cancellationToken);
        return HandleResult(result, "Failed to retrieve invitation detail");
    }

    [HttpPost("invitations/{id:guid}/resend")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendInvitation(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new ResendInvitationCommand(id), cancellationToken);
        return HandleNoContent(result, "Failed to resend invitation");
    }

    [HttpPost("invitations/{token}/accept-with-registration")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AcceptInvitationWithRegistration(
        string token,
        [FromBody] AcceptInvitationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AcceptInvitationWithRegistrationCommand(
            token, request.FirstName, request.LastName,
            request.UserName, request.Password, request.PhoneNumber, request.Gender);
        var result = await Sender.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : HandleResult(result, "Invitation Failed");
    }

    [HttpDelete("invitations/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelInvitation(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new CancelInvitationCommand(id), cancellationToken);
        return HandleNoContent(result, "Failed to cancel invitation");
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<StaffDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStaffList(
        [FromQuery] string? role,
        [FromQuery] bool? isActive,
        [FromQuery] SortedPaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStaffListQuery(role, isActive, pagination.SortBy, pagination.SortDirection, pagination.PageNumber, pagination.PageSize);
        var result = await Sender.Send(query, cancellationToken);
        return HandleResult(result, "Failed to retrieve staff");
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StaffDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStaffDetail(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetStaffDetailQuery(id), cancellationToken);
        return HandleResult(result, "Failed to retrieve staff detail");
    }

    [HttpPost("set-owner-as-doctor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetOwnerAsDoctor(
        [FromBody] SetOwnerAsDoctorRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new SetOwnerAsDoctorCommand(request.SpecializationId), cancellationToken);
        return HandleNoContent(result, "Failed to set owner as doctor");
    }

    [HttpPatch("{id:guid}/active-status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetActiveStatus(
        Guid id, [FromBody] SetStaffActiveStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new SetStaffActiveStatusCommand(id, request.IsActive), cancellationToken);
        return HandleNoContent(result, "Failed to update staff status");
    }

    [HttpGet("{id:guid}/working-days")]
    [ProducesResponseType(typeof(List<WorkingDayDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWorkingDays(Guid id, [FromQuery] Guid? branchId, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetWorkingDaysQuery(id, branchId), cancellationToken);
        return HandleResult(result, "Failed to retrieve working days");
    }

    [HttpPut("{id:guid}/working-days")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveWorkingDays(
        Guid id, [FromBody] SaveWorkingDaysRequest request, CancellationToken cancellationToken)
    {
        if (request.BranchId == Guid.Empty)
            return BadRequest("BranchId is required");
        var days = request.Days.Select(d => new WorkingDayInput(d.Day, d.StartTime, d.EndTime, d.IsAvailable)).ToList();
        var result = await Sender.Send(new SaveWorkingDaysCommand(id, request.BranchId, days), cancellationToken);
        return HandleNoContent(result, "Failed to save working days");
    }
}
