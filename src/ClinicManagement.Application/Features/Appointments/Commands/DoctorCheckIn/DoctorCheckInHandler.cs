using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

public class DoctorCheckInHandler : IRequestHandler<DoctorCheckInCommand, Result<DoctorCheckInResult>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DoctorCheckInHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<DoctorCheckInResult>> Handle(DoctorCheckInCommand request, CancellationToken ct)
    {
        var clinicId = _currentUser.GetRequiredClinicId();

        var nowUtc = DateTimeOffset.UtcNow;
        // Use local date for the session so it matches what the user sees in the UI
        var today  = DateOnly.FromDateTime(nowUtc.ToLocalTime().Date);

        // Prevent duplicate check-in
        var existing = await _uow.DoctorSessions.GetByDoctorBranchDateAsync(
            request.DoctorInfoId, request.BranchId, today, ct);

        if (existing is not null)
            return Result.Failure<DoctorCheckInResult>(ErrorCodes.ALREADY_EXISTS, "Doctor already checked in today");

        // Get scheduled start time from working days
        var schedule   = await _uow.DoctorSchedules.GetScheduleAsync(request.DoctorInfoId, request.BranchId, ct);
        var todayDow   = nowUtc.ToLocalTime().DayOfWeek;
        var workingDay = schedule?.WorkingDays.FirstOrDefault(w => w.Day == todayDow && w.IsAvailable);

        // Build the absolute UTC moment the doctor was scheduled to start.
        // WorkingDay.StartTime is a local clock time (e.g. 09:00).
        // We combine today's local date with that clock time, then convert to UTC.
        // This is timezone-safe: the offset is captured at the moment of check-in.
        DateTimeOffset? scheduledStartUtc = null;
        if (workingDay is not null)
        {
            var localOffset = nowUtc.ToLocalTime().Offset;
            var scheduledLocal = new DateTimeOffset(
                today.Year, today.Month, today.Day,
                workingDay.StartTime.Hour, workingDay.StartTime.Minute, 0,
                localOffset);
            scheduledStartUtc = scheduledLocal.ToUniversalTime();
        }

        var session = new DoctorSession
        {
            ClinicId           = clinicId,
            DoctorInfoId       = request.DoctorInfoId,
            BranchId           = request.BranchId,
            Date               = today,
            CheckedInAt        = nowUtc,
            ScheduledStartUtc  = scheduledStartUtc,
            ScheduledStartTime = workingDay?.StartTime,  // kept for display
        };

        await _uow.DoctorSessions.AddAsync(session, ct);
        await _uow.SaveChangesAsync(ct);

        // Compute and store delay once — pure UTC arithmetic
        var delayMinutes = session.DelayMinutes;
        var isLate       = delayMinutes.HasValue && delayMinutes > 0;

        if (isLate)
        {
            session.StoredDelayMinutes = delayMinutes;
            await _uow.SaveChangesAsync(ct);
        }

        return Result.Success(new DoctorCheckInResult(
            session.Id,
            isLate,
            delayMinutes,
            workingDay?.StartTime.ToString("HH:mm")  // local clock time for display
        ));
    }
}
