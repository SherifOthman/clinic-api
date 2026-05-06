using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

public class DoctorCheckOutHandler : IRequestHandler<DoctorCheckOutCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public DoctorCheckOutHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(DoctorCheckOutCommand request, CancellationToken ct)
    {
        var nowUtc = DateTimeOffset.UtcNow;
        var today  = DateOnly.FromDateTime(nowUtc.ToLocalTime().Date);

        var session = await _uow.DoctorSessions.GetByDoctorBranchDateAsync(
            request.DoctorInfoId, request.BranchId, today, ct);

        if (session is null || !session.IsActive)
            return Result.Failure(ErrorCodes.NOT_FOUND, "No active session found for this doctor today");

        // End the session
        session.CheckedOutAt = nowUtc;

        // Auto-complete any appointments still InProgress — doctor has left
        var appointments = await _uow.Appointments.GetByDoctorAndDateForUpdateAsync(
            request.DoctorInfoId, today, ct);

        foreach (var appt in appointments.Where(a => a.Status == AppointmentStatus.InProgress))
            appt.Status = AppointmentStatus.Completed;

        _uow.DoctorSessions.Update(session);
        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
