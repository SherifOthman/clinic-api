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

        // Use UTC date so the session date is consistent regardless of server timezone
        var nowUtc = DateTimeOffset.UtcNow;
        var today  = DateOnly.FromDateTime(nowUtc.Date);

        // Prevent duplicate check-in
        var existing = await _uow.DoctorSessions.GetByDoctorBranchDateAsync(
            request.DoctorInfoId, request.BranchId, today, ct);

        if (existing is not null)
            return Result.Failure<DoctorCheckInResult>(ErrorCodes.ALREADY_EXISTS, "Doctor already checked in today");

        // Get scheduled start time from working days
        var schedule   = await _uow.DoctorSchedules.GetScheduleAsync(request.DoctorInfoId, request.BranchId, ct);
        var todayDow   = nowUtc.DayOfWeek;
        var workingDay = schedule?.WorkingDays.FirstOrDefault(w => w.Day == todayDow && w.IsAvailable);

        var session = new DoctorSession
        {
            ClinicId           = clinicId,
            DoctorInfoId       = request.DoctorInfoId,
            BranchId           = request.BranchId,
            Date               = today,
            CheckedInAt        = nowUtc,
            ScheduledStartTime = workingDay?.StartTime,
        };

        await _uow.DoctorSessions.AddAsync(session, ct);
        await _uow.SaveChangesAsync(ct);

        // Compute delay using the UTC-based property
        var delayMinutes = session.DelayMinutes;
        var isLate       = delayMinutes.HasValue && delayMinutes > 0;

        // If the doctor is not late, the session has no actionable value — remove it.
        // The frontend will show "Session Active" only when there's a real session.
        // Actually keep the session so hasSessionToday works — just don't store delay.
        // Store delay for HandleDelayHandler to use
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
