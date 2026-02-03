using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Appointments.Commands.CreateAppointment;
using ClinicManagement.Application.Features.Appointments.Queries.GetAppointments;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : BaseApiController
{
    private readonly IAppointmentTypeRepository _appointmentTypeRepository;

    public AppointmentsController(IMediator mediator, IAppointmentTypeRepository appointmentTypeRepository) : base(mediator)
    {
        _appointmentTypeRepository = appointmentTypeRepository;
    }

    /// <summary>
    /// Get appointments with optional filters
    /// </summary>
    /// <param name="date">Filter by appointment date</param>
    /// <param name="doctorId">Filter by doctor ID</param>
    /// <param name="patientId">Filter by patient ID</param>
    /// <param name="appointmentTypeId">Filter by appointment type ID</param>
    /// <param name="status">Filter by appointment status</param>
    /// <returns>List of appointments</returns>
    [HttpGet]
    public async Task<IActionResult> GetAppointments(
        [FromQuery] DateTime? date = null,
        [FromQuery] Guid? doctorId = null,
        [FromQuery] Guid? patientId = null,
        [FromQuery] Guid? appointmentTypeId = null,
        [FromQuery] AppointmentStatus? status = null)
    {
        var query = new GetAppointmentsQuery(date, doctorId, patientId, appointmentTypeId, status);
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new appointment
    /// </summary>
    /// <param name="createDto">Appointment data</param>
    /// <returns>Created appointment</returns>
    [HttpPost]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDto createDto)
    {
        var command = new CreateAppointmentCommand(createDto);
        var result = await Mediator.Send(command);
        return HandleCreateResult(result, nameof(GetAppointments), null);
    }

    /// <summary>
    /// Get appointment types
    /// </summary>
    /// <returns>List of appointment types</returns>
    [HttpGet("types")]
    public async Task<IActionResult> GetAppointmentTypes()
    {
        var appointmentTypes = await _appointmentTypeRepository.GetActiveAsync();
        return Ok(appointmentTypes);
    }
}
