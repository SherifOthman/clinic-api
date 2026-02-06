using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Queries.GetAppointmentTypes;

public record GetAppointmentTypesQuery : IRequest<Result<IEnumerable<AppointmentTypeDto>>>;
