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

        // Use tracked load — no navigation includes, mutations detected automatically
        var appointments = await _uow.Appointments.GetByDoctorAndDateForUpdateAsync(
            session.DoctorInfoId, session.Date, ct);

        // Current local time-of-day for comparisons (appointments store local clock times)
        var nowLocalTime = TimeOnly.FromDateTime(DateTimeOffset.Now.LocalDateTime);

        switch (request.Option)
        {
            case DelayHandlingOption.AutoShift:
            {
                // Strategy: reschedule from NOW, preserving spacing between appointments.
                //
                // 1. Collect all pending/waiting time appointments, sorted by original time.
                // 2. The first one starts at now + 5 min (give the doctor a moment to settle).
                // 3. Each subsequent appointment keeps its original duration gap from the previous.
                //
                // Example: appointments at 2:00, 2:30, 3:00, 3:30 (30-min gaps).
                // Doctor arrives at 4:29. Result: 4:34, 5:04, 5:34, 6:04.

                var toShift = appointments
                    .Where(a =>
                        a.Type == AppointmentType.Time &&
                        (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Waiting) &&
                        a.ScheduledTime.HasValue)
                    .OrderBy(a => a.ScheduledTime!.Value)
                    .ToList();

                if (toShift.Count > 0)
                {
                    // First appointment starts 5 minutes from now
                    var firstNewStart = nowLocalTime.AddMinutes(5);
                    var originalFirst = toShift[0].ScheduledTime!.Value;

                    // Compute how much to shift: difference between new first start and original first
                    var shiftMinutes = (int)(firstNewStart - originalFirst).TotalMinutes;
                    // Handle midnight wrap
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
                // Mark pending appointments whose scheduled time has already passed as NoShow.
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

        _uow.DoctorSessions.Update(session);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
