using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Queries.GetAppointmentById;

public record GetAppointmentByIdQuery : IRequest<Result<AppointmentDto>>
{
    public int Id { get; set; }
}
