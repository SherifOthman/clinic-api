using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Queries.GetAppointments;

public record GetAppointmentsQuery(
    DateTime? Date = null,
    Guid? DoctorId = null,
    Guid? PatientId = null,
    Guid? AppointmentTypeId = null,
    AppointmentStatus? Status = null
) : IRequest<Result<IEnumerable<AppointmentDto>>>;
