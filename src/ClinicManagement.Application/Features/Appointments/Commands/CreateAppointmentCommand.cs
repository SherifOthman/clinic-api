using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

/// <summary>
/// Creates either a Queue or Time appointment.
/// For Queue: QueueNumber is auto-assigned; ScheduledTime must be null.
/// For Time:  ScheduledTime is required; QueueNumber is auto-null.
/// </summary>
public record CreateAppointmentCommand(
    Guid BranchId,
    Guid PatientId,
    Guid DoctorInfoId,
    Guid VisitTypeId,
    DateOnly Date,
    AppointmentType Type,
    TimeOnly? ScheduledTime,
    decimal? DiscountPercent
) : IRequest<Result<Guid>>;
