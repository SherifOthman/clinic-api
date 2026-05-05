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

        if (session.DelayHandling.HasValue)
            return Result.Failure(ErrorCodes.ALREADY_EXISTS, "Delay already handled");

        // Cancel = user dismissed the dialog — delete the session as if check-in never happened
        if (request.Option == DelayHandlingOption.Cancel)
        {
            _uow.DoctorSessions.Delete(session);
            await _uow.SaveChangesAsync(ct);
            return Result.Success();
        }

        session.DelayHandling = request.Option;

        var delayMinutes = session.StoredDelayMinutes ?? session.DelayMinutes ?? 0;

        var appointments = await _uow.Appointments.GetByDoctorAndDateAsync(
            session.DoctorInfoId, session.Date, ct);

        switch (request.Option)
        {
            case DelayHandlingOption.AutoShift:
                foreach (var appt in appointments.Where(a =>
                    a.Type == AppointmentType.Time &&
                    (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Waiting) &&
                    a.ScheduledTime.HasValue))
                {
                    appt.ScheduledTime = appt.ScheduledTime!.Value.AddMinutes(delayMinutes);
                    if (appt.EndTime.HasValue)
                        appt.EndTime = appt.EndTime.Value.AddMinutes(delayMinutes);
                    _uow.Appointments.Update(appt);
                }
                break;

            case DelayHandlingOption.MarkMissed:
                var nowUtcTime = TimeOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
                foreach (var appt in appointments.Where(a =>
                    a.Status == AppointmentStatus.Pending &&
                    a.ScheduledTime.HasValue &&
                    a.ScheduledTime.Value < nowUtcTime))
                {
                    appt.Status = AppointmentStatus.NoShow;
                    _uow.Appointments.Update(appt);
                }
                break;

            case DelayHandlingOption.Manual:
                break;
        }

        _uow.DoctorSessions.Update(session);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
