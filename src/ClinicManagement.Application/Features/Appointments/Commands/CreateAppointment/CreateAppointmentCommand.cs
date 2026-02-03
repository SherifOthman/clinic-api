using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands.CreateAppointment;

public record CreateAppointmentCommand(
    CreateAppointmentDto Appointment
) : IRequest<Result<AppointmentDto>>;
