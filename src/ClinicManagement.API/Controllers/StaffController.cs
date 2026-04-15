using ClinicManagement.API.Contracts.Staff;
using ClinicManagement.API.Models;
using ClinicManagement.API.RateLimiting;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Features.Staff.Commands;
using ClinicManagement.Application.Features.Staff.Dtos;
using ClinicManagement.Application.Features.Staff.Queries;
using ClinicManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicManagement.API.Controllers;

[Route("api/staff")]
public class StaffController : BaseApiController
{
    // ── Clinic-owner-only endpoints ───────────────────────────────────────────

    [Authorize(Policy = "RequireClinicOwner")]
    [HttpPost("invite")]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(typeof(InviteStaffResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InviteStaff(
        [FromBody] InviteStaffRequest request, CancellationToken cancellationToken)
    {
        var command = new InviteStaffCommand(request.Role, request.Email, request.SpecializationId);
        var result = await Sender.Send(command, cancellationToken);
        return !result.IsSuccess ? HandleResult(result, "Failed to send invitation") : Ok(result.Value);
    }

    [Authorize(Policy = "RequireClinicOwner")]
    [HttpGet("invitations")]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
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

    [Authorize(Policy = "RequireClinicOwner")]
    [HttpGet("invitations/{id:guid}")]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
    [ProducesResponseType(typeof(InvitationDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvitationDetail(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetInvitationDetailQuery(id), cancellationToken);
        return HandleResult(result, "Failed to retrieve invitation detail");
    }

    [Authorize(Policy = "RequireClinicOwner")]
    [HttpPost("invitations/{id:guid}/resend")]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendInvitation(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new ResendInvitationCommand(id), cancellationToken);
        return HandleNoContent(result, "Failed to resend invitation");
    }

    [HttpPost("invitations/{token}/accept-with-registration")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.UserOnce)]
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

    [Authorize(Policy = "RequireClinicOwner")]
    [HttpDelete("invitations/{id}")]
    [EnableRateLimiting(RateLimitPolicies.UserDeletes)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelInvitation(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new CancelInvitationCommand(id), cancellationToken);
        return HandleNoContent(result, "Failed to cancel invitation");
    }

    [Authorize(Policy = "RequireClinicOwner")]
    [HttpGet]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
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

    [Authorize(Policy = "RequireClinicOwner")]
    [HttpGet("{id:guid}")]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
    [ProducesResponseType(typeof(StaffDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStaffDetail(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetStaffDetailQuery(id), cancellationToken);
        return HandleResult(result, "Failed to retrieve staff detail");
    }

    [Authorize(Policy = "RequireClinicOwner")]
    [HttpPost("set-owner-as-doctor")]
    [EnableRateLimiting(RateLimitPolicies.UserOnce)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetOwnerAsDoctor(
        [FromBody] SetOwnerAsDoctorRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new SetOwnerAsDoctorCommand(request.SpecializationId), cancellationToken);
        return HandleNoContent(result, "Failed to set owner as doctor");
    }

    [Authorize(Policy = "RequireClinicOwner")]
    [HttpPatch("{id:guid}/active-status")]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetActiveStatus(
        Guid id, [FromBody] SetStaffActiveStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new SetStaffActiveStatusCommand(id, request.IsActive), cancellationToken);
        return HandleNoContent(result, "Failed to update staff status");
    }

    // ── Schedule endpoints — accessible by clinic owner OR the doctor themselves ──

    [Authorize(Policy = "RequireClinic")]
    [HttpGet("{id:guid}/working-days")]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
    [ProducesResponseType(typeof(List<WorkingDayDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWorkingDays(Guid id, [FromQuery] Guid? branchId, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetWorkingDaysQuery(id, branchId), cancellationToken);
        return HandleResult(result, "Failed to retrieve working days");
    }

    [Authorize(Policy = "RequireClinic")]
    [HttpPut("{id:guid}/working-days")]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
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

    [Authorize(Policy = "RequireClinic")]
    [HttpGet("{id:guid}/visit-types")]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
    [ProducesResponseType(typeof(List<DoctorVisitTypeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVisitTypes(Guid id, [FromQuery] Guid branchId, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetDoctorVisitTypesQuery(id, branchId), cancellationToken);
        return HandleResult(result, "Failed to retrieve visit types");
    }

    [Authorize(Policy = "RequireClinic")]
    [HttpPut("{id:guid}/visit-types")]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpsertVisitType(
        Guid id, [FromBody] UpsertDoctorVisitTypeRequest request, CancellationToken cancellationToken)
    {
        var command = new UpsertDoctorVisitTypeCommand(id, request.BranchId, request.VisitTypeId, request.NameAr, request.NameEn, request.Price, request.IsActive);
        var result = await Sender.Send(command, cancellationToken);
        return !result.IsSuccess ? HandleResult(result, "Failed to save visit type") : Ok(result.Value);
    }

    [Authorize(Policy = "RequireClinic")]
    [HttpDelete("{id:guid}/visit-types/{visitTypeId:guid}")]
    [EnableRateLimiting(RateLimitPolicies.UserDeletes)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveVisitType(Guid id, Guid visitTypeId, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new RemoveDoctorVisitTypeCommand(visitTypeId), cancellationToken);
        return HandleNoContent(result, "Failed to remove visit type");
    }

    // ── Clinic-owner-only: lock/unlock doctor self-management ─────────────────

    [Authorize(Policy = "RequireClinicOwner")]
    [HttpPatch("{id:guid}/schedule-lock")]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetScheduleLock(
        Guid id, [FromBody] SetDoctorScheduleLockRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new SetDoctorScheduleLockCommand(id, request.CanSelfManage), cancellationToken);
        return HandleNoContent(result, "Failed to update schedule lock");
    }
}
