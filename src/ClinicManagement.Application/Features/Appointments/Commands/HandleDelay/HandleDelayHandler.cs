using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

public class HandleDelayHandler : IRequestHandler<HandleDelayCommand, Result>
{
    private readonly IDoctorSessionRepository _sessions;
    private readonly IAppointmentRepository   _appointments;
    private readonly IUnitOfWork _uow;

    public HandleDelayHandler(
        IDoctorSessionRepository sessions,
        IAppointmentRepository appointments,
        IUnitOfWork uow)
    {
        _sessions     = sessions;
        _appointments = appointments;
        _uow          = uow;
    }

    public async Task<Result> Handle(HandleDelayCommand request, CancellationToken ct)
    {
        var session = await _sessions.GetByIdAsync(request.SessionId, ct);
        if (session is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Session not found");

        if (session.DelayHandling.HasValue)
            return Result.Failure(ErrorCodes.ALREADY_EXISTS, "Delay already handled");

        if (request.Option == DelayHandlingOption.Cancel)
        {
            _sessions.Delete(session);
            await _uow.SaveChangesAsync(ct);
            return Result.Success();
        }

        session.DelayHandling = request.Option;

        var delayMinutes = session.StoredDelayMinutes ?? session.DelayMinutes ?? 0;

        var appointments = await _appointments.GetByDoctorAndDateForUpdateAsync(
            session.DoctorInfoId, session.Date, ct);

        var nowLocalTime = TimeOnly.FromDateTime(DateTimeOffset.Now.LocalDateTime);

        switch (request.Option)
        {
            case DelayHandlingOption.AutoShift:
            {
                var toShift = appointments
                    .Where(a =>
                        a.Type == AppointmentType.Time &&
                        (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Waiting) &&
                        a.ScheduledTime.HasValue)
                    .OrderBy(a => a.ScheduledTime!.Value)
                    .ToList();

                if (toShift.Count > 0)
                {
                    var firstNewStart = nowLocalTime.AddMinutes(5);
                    var originalFirst = toShift[0].ScheduledTime!.Value;
                    var shiftMinutes  = (int)(firstNewStart - originalFirst).TotalMinutes;
                    if (shiftMinutes < -720) shiftMinutes += 1440;

                    foreach (var appt in toShift)
                    {
                        appt.ScheduledTime = appt.ScheduledTime!.Value.AddMinutes(shiftMinutes);
                        if (appt.EndTime.HasValue)
                            appt.EndTime = appt.EndTime.Value.AddMinutes(shiftMinutes);
                    }
                }
                break;
            }

            case DelayHandlingOption.MarkMissed:
                foreach (var appt in appointments.Where(a =>
                    a.Status == AppointmentStatus.Pending &&
                    a.ScheduledTime.HasValue &&
                    a.ScheduledTime.Value < nowLocalTime))
                {
                    appt.Status = AppointmentStatus.NoShow;
                }
                break;

            case DelayHandlingOption.Manual:
                break;
        }

        _sessions.Update(session);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
