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

        if (appt.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled)
            return Result.Failure(ErrorCodes.OPERATION_NOT_ALLOWED,
                "Cannot reschedule a completed or cancelled appointment");

        var today = DateOnly.FromDateTime(DateTimeOffset.Now.LocalDateTime.Date);
        if (request.NewDate <= today)
            return Result.Failure(ErrorCodes.VALIDATION_ERROR, "New date must be in the future");

        // Load existing pending/waiting appointments on the target date
        var existing = await _uow.Appointments.GetByDoctorDatePendingForUpdateAsync(
            appt.DoctorInfoId, request.NewDate, ct);

        // The carry-over patient goes FIRST (queue #1).
        // All existing patients on that day shift down by 1.
        appt.Date        = request.NewDate;
        appt.QueueNumber = 1;

        if (appt.Status == AppointmentStatus.Waiting)
            appt.Status = AppointmentStatus.Pending;

        // Push existing patients back: #1→#2, #2→#3, etc.
        foreach (var e in existing.OrderBy(e => e.QueueNumber ?? 0))
            e.QueueNumber = (e.QueueNumber ?? 0) + 1;

        _uow.Appointments.Update(appt);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
