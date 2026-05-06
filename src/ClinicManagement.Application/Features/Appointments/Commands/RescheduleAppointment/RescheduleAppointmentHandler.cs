using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

public class RescheduleAppointmentHandler : IRequestHandler<RescheduleAppointmentCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public RescheduleAppointmentHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(RescheduleAppointmentCommand request, CancellationToken ct)
    {
        var appt = await _uow.Appointments.GetByIdForUpdateAsync(request.AppointmentId, ct);
        if (appt is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Appointment not found");

        // Only active appointments can be rescheduled
        if (appt.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled)
            return Result.Failure(ErrorCodes.OPERATION_NOT_ALLOWED,
                "Cannot reschedule a completed or cancelled appointment");

        // Must be a future date
        var today = DateOnly.FromDateTime(DateTimeOffset.Now.LocalDateTime.Date);
        if (request.NewDate <= today)
            return Result.Failure(ErrorCodes.VALIDATION_ERROR, "New date must be in the future");

        appt.Date = request.NewDate;

        // If patient was waiting, reset to Pending — they haven't been called yet on the new day
        if (appt.Status == AppointmentStatus.Waiting)
            appt.Status = AppointmentStatus.Pending;

        _uow.Appointments.Update(appt);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
