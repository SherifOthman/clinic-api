using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

public class DoctorCheckOutHandler : IRequestHandler<DoctorCheckOutCommand, Result>
{
    private readonly IDoctorSessionRepository  _sessions;
    private readonly IAppointmentRepository    _appointments;
    private readonly IUnitOfWork _uow;

    public DoctorCheckOutHandler(
        IDoctorSessionRepository sessions,
        IAppointmentRepository appointments,
        IUnitOfWork uow)
    {
        _sessions     = sessions;
        _appointments = appointments;
        _uow          = uow;
    }

    public async Task<Result> Handle(DoctorCheckOutCommand request, CancellationToken ct)
    {
        var nowUtc = DateTimeOffset.UtcNow;
        var today  = DateOnly.FromDateTime(nowUtc.ToLocalTime().Date);

        var session = await _sessions.GetByDoctorBranchDateAsync(
            request.DoctorInfoId, request.BranchId, today, ct);

        if (session is null || !session.IsActive)
            return Result.Failure(ErrorCodes.NOT_FOUND, "No active session found for this doctor today");

        session.CheckedOutAt = nowUtc;

        var appointments = await _appointments.GetByDoctorAndDateForUpdateAsync(
            request.DoctorInfoId, today, ct);

        foreach (var appt in appointments.Where(a => a.Status == AppointmentStatus.InProgress))
            appt.Status = AppointmentStatus.Completed;

        _sessions.Update(session);
        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
