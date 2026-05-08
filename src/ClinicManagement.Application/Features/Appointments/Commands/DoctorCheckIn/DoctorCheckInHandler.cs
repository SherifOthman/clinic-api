using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

public class DoctorCheckInHandler : IRequestHandler<DoctorCheckInCommand, Result<DoctorCheckInResult>>
{
    private readonly IDoctorSessionRepository  _sessions;
    private readonly IDoctorScheduleRepository _schedules;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DoctorCheckInHandler(
        IDoctorSessionRepository sessions,
        IDoctorScheduleRepository schedules,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _sessions    = sessions;
        _schedules   = schedules;
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<DoctorCheckInResult>> Handle(DoctorCheckInCommand request, CancellationToken ct)
    {
        var clinicId = _currentUser.GetRequiredClinicId();
        var nowUtc   = DateTimeOffset.UtcNow;
        var today    = DateOnly.FromDateTime(nowUtc.ToLocalTime().Date);

        var existing = await _sessions.GetByDoctorBranchDateAsync(
            request.DoctorInfoId, request.BranchId, today, ct);

        if (existing is not null)
            return Result.Failure<DoctorCheckInResult>(ErrorCodes.ALREADY_EXISTS, "Doctor already checked in today");

        var schedule   = await _schedules.GetScheduleAsync(request.DoctorInfoId, request.BranchId, ct);
        var todayDow   = nowUtc.ToLocalTime().DayOfWeek;
        var workingDay = schedule?.WorkingDays.FirstOrDefault(w => w.Day == todayDow && w.IsAvailable);

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
            ScheduledStartTime = workingDay?.StartTime,
        };

        await _sessions.AddAsync(session, ct);
        await _uow.SaveChangesAsync(ct);

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
            workingDay?.StartTime.ToString("HH:mm")
        ));
    }
}
