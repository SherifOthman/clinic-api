using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

public class HandleDelayHandler : IRequestHandler<HandleDelayCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public HandleDelayHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(HandleDelayCommand request, CancellationToken ct)
    {
        var session = await _uow.DoctorSessions.GetByIdAsync(request.SessionId, ct);
        if (session is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Session not found");

        if (!session.IsLate)
            return Result.Failure(ErrorCodes.OPERATION_NOT_ALLOWED, "Doctor is not late");

        if (session.DelayHandling.HasValue)
            return Result.Failure(ErrorCodes.ALREADY_EXISTS, "Delay already handled");

        session.DelayHandling = request.Option;

        var delayMinutes = session.DelayMinutes ?? 0;
        var appointments = await _uow.Appointments.GetByDoctorAndDateAsync(
            session.DoctorInfoId, session.Date, ct);

        switch (request.Option)
        {
            case DelayHandlingOption.AutoShift:
                // Shift all pending/waiting time-based appointments forward
                foreach (var appt in appointments.Where(a =>
                    a.Type == AppointmentType.Time &&
                    (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Waiting)))
                {
                    if (appt.ScheduledTime.HasValue)
                    {
                        appt.ScheduledTime = appt.ScheduledTime.Value.AddMinutes(delayMinutes);
                        if (appt.EndTime.HasValue)
                            appt.EndTime = appt.EndTime.Value.AddMinutes(delayMinutes);
                        _uow.Appointments.Update(appt);
                    }
                }
                break;

            case DelayHandlingOption.MarkMissed:
                // Mark all past pending appointments as NoShow
                var now = TimeOnly.FromDateTime(DateTime.Now);
                foreach (var appt in appointments.Where(a =>
                    a.Status == AppointmentStatus.Pending &&
                    a.ScheduledTime.HasValue &&
                    a.ScheduledTime.Value < now))
                {
                    appt.Status = AppointmentStatus.NoShow;
                    _uow.Appointments.Update(appt);
                }
                break;

            case DelayHandlingOption.Manual:
                // No automatic changes — receptionist handles each appointment
                break;
        }

        _uow.DoctorSessions.Update(session);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
