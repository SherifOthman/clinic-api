using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

public class UpdateAppointmentStatusHandler : IRequestHandler<UpdateAppointmentStatusCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public UpdateAppointmentStatusHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(UpdateAppointmentStatusCommand request, CancellationToken ct)
    {
        var appt = await _uow.Appointments.GetByIdForUpdateAsync(request.Id, ct);
        if (appt is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Appointment not found");

        // Validate transitions
        var valid = (appt.Status, request.Status) switch
        {
            (AppointmentStatus.Pending,    AppointmentStatus.InProgress) => true,
            (AppointmentStatus.Pending,    AppointmentStatus.Cancelled)  => true,
            (AppointmentStatus.Pending,    AppointmentStatus.NoShow)     => true,
            (AppointmentStatus.InProgress, AppointmentStatus.Completed)  => true,
            (AppointmentStatus.InProgress, AppointmentStatus.Cancelled)  => true,
            _ => false,
        };

        if (!valid)
            return Result.Failure(ErrorCodes.OPERATION_NOT_ALLOWED,
                $"Cannot transition from {appt.Status} to {request.Status}");

        appt.Status = request.Status;
        _uow.Appointments.Update(appt);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
