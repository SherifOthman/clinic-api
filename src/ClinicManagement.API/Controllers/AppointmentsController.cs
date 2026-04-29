using ClinicManagement.API.Authorization;
using ClinicManagement.API.Contracts.Appointments;
using ClinicManagement.API.Models;
using ClinicManagement.API.RateLimiting;
using ClinicManagement.Application.Features.Appointments.Commands;
using ClinicManagement.Application.Features.Appointments.Queries;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicManagement.API.Controllers;

[Route("api/appointments")]
[Authorize]
public class AppointmentsController : BaseApiController
{
    // ── Queries ───────────────────────────────────────────────────────────────

    /// <summary>Get appointments for one or more doctors on a date.</summary>
    [HttpGet]
    [RequirePermission(Permission.ViewAppointments)]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
    [ProducesResponseType(typeof(List<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAppointments(
        [FromQuery] string date,
        [FromQuery] Guid? branchId,
        [FromQuery] List<Guid>? doctorInfoIds,
        CancellationToken ct)
    {
        if (!DateOnly.TryParse(date, out var parsedDate))
            return BadRequest("Invalid date format. Use YYYY-MM-DD.");

        var result = await Sender.Send(new GetAppointmentsQuery(parsedDate, branchId, doctorInfoIds), ct);
        return Ok(result);
    }

    /// <summary>Get all doctors available at a branch (for the doctor selector).</summary>
    [HttpGet("doctors")]
    [RequirePermission(Permission.ViewAppointments)]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDoctors([FromQuery] Guid branchId, CancellationToken ct)
    {
        var result = await Sender.Send(new GetDoctorsForBranchQuery(branchId), ct);
        return Ok(result);
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    /// <summary>Book a new appointment (queue or time-based).</summary>
    [HttpPost]
    [RequirePermission(Permission.ManageAppointments)]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request, CancellationToken ct)
    {
        if (!DateOnly.TryParse(request.Date, out var date))
            return BadRequest("Invalid date format. Use YYYY-MM-DD.");

        if (!Enum.TryParse<AppointmentType>(request.Type, out var type))
            return BadRequest("Invalid appointment type. Use 'Queue' or 'Time'.");

        TimeOnly? scheduledTime = null;
        if (request.ScheduledTime is not null)
        {
            if (!TimeOnly.TryParse(request.ScheduledTime, out var parsedTime))
                return BadRequest("Invalid time format. Use HH:mm.");
            scheduledTime = parsedTime;
        }

        var command = new CreateAppointmentCommand(
            request.BranchId, request.PatientId, request.DoctorInfoId,
            request.VisitTypeId, date, type, scheduledTime, request.DiscountPercent,
            request.VisitDurationMinutes);

        var result = await Sender.Send(command, ct);
        if (result.IsFailure) return HandleResult(result, "Failed to create appointment");
        return CreatedAtAction(nameof(GetAppointments), new { date = request.Date }, result.Value);
    }

    /// <summary>Update appointment status (Pending→InProgress→Completed, etc.).</summary>
    [HttpPatch("{id:guid}/status")]
    [RequirePermission(Permission.ManageAppointments)]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<AppointmentStatus>(request.Status, out var status))
            return BadRequest("Invalid status.");

        var result = await Sender.Send(new UpdateAppointmentStatusCommand(id, status), ct);
        return HandleNoContent(result, "Failed to update status");
    }

    /// <summary>Set whether a doctor uses Queue or Time appointments (owner only).</summary>
    [HttpPatch("doctors/{memberId:guid}/appointment-type")]
    [Authorize(Policy = "RequireClinicOwner")]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetAppointmentType(Guid memberId, [FromBody] SetAppointmentTypeRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<AppointmentType>(request.AppointmentType, out var apptType))
            return BadRequest("Invalid appointment type. Use 'Queue' or 'Time'.");

        var result = await Sender.Send(new SetDoctorAppointmentTypeCommand(memberId, apptType), ct);
        return HandleNoContent(result, "Failed to set appointment type");
    }

    // ── Doctor sessions ───────────────────────────────────────────────────────

    /// <summary>Doctor checks in for the day. Returns delay info if late.</summary>
    [HttpPost("sessions/check-in")]
    [RequirePermission(Permission.ManageAppointments)]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(typeof(DoctorCheckInResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new DoctorCheckInCommand(request.DoctorInfoId, request.BranchId), ct);
        return result.IsFailure ? HandleResult(result, "Check-in failed") : Ok(result.Value);
    }

    /// <summary>Handle a doctor delay — auto-shift, mark missed, or manual.</summary>
    [HttpPost("sessions/{sessionId:guid}/handle-delay")]
    [RequirePermission(Permission.ManageAppointments)]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> HandleDelay(Guid sessionId, [FromBody] HandleDelayRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<DelayHandlingOption>(request.Option, out var option))
            return BadRequest("Invalid option. Use 'AutoShift', 'MarkMissed', or 'Manual'.");

        var result = await Sender.Send(new HandleDelayCommand(sessionId, option), ct);
        return HandleNoContent(result, "Failed to handle delay");
    }
}
