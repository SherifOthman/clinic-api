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
        var today    = DateOnly.FromDateTime(DateTime.Today);

        // Prevent duplicate check-in
        var existing = await _uow.DoctorSessions.GetByDoctorBranchDateAsync(
            request.DoctorInfoId, request.BranchId, today, ct);

        if (existing is not null)
            return Result.Failure<DoctorCheckInResult>(ErrorCodes.ALREADY_EXISTS, "Doctor already checked in today");

        // Get scheduled start time from working days
        var schedule = await _uow.DoctorSchedules.GetScheduleAsync(request.DoctorInfoId, request.BranchId, ct);
        var todayDow = DateTime.Today.DayOfWeek;
        var workingDay = schedule?.WorkingDays.FirstOrDefault(w => w.Day == todayDow && w.IsAvailable);

        var session = new DoctorSession
        {
            ClinicId            = clinicId,
            DoctorInfoId        = request.DoctorInfoId,
            BranchId            = request.BranchId,
            Date                = today,
            CheckedInAt         = DateTimeOffset.UtcNow,
            ScheduledStartTime  = workingDay?.StartTime,
        };

        await _uow.DoctorSessions.AddAsync(session, ct);
        await _uow.SaveChangesAsync(ct);

        // Store computed delay so HandleDelayHandler can use it reliably
        session.StoredDelayMinutes = session.DelayMinutes;
        await _uow.SaveChangesAsync(ct);

        return Result.Success(new DoctorCheckInResult(
            session.Id,
            session.IsLate,
            session.DelayMinutes,
            workingDay?.StartTime.ToString("HH:mm")
        ));
    }
}
