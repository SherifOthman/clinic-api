using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

/// <summary>
/// Updates an existing appointment's visit type, scheduled time, discount, and duration.
/// Status and type cannot be changed here — use the dedicated status endpoint.
/// </summary>
public record UpdateAppointmentCommand(
    Guid AppointmentId,
    Guid VisitTypeId,
    TimeOnly? ScheduledTime,
    decimal? DiscountPercent,
    int? VisitDurationMinutes
) : IRequest<Result>;
