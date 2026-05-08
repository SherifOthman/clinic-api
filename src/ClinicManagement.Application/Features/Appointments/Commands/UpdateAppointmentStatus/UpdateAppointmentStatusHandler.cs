using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

public record UpdateAppointmentStatusCommand(Guid Id, AppointmentStatus Status) : IRequest<Result>;

public class UpdateAppointmentStatusHandler : IRequestHandler<UpdateAppointmentStatusCommand, Result>
{
    private readonly IAppointmentRepository _appointments;
    private readonly IUnitOfWork _uow;

    public UpdateAppointmentStatusHandler(IAppointmentRepository appointments, IUnitOfWork uow)
    {
        _appointments = appointments;
        _uow          = uow;
    }

    public async Task<Result> Handle(UpdateAppointmentStatusCommand request, CancellationToken ct)
    {
        var appt = await _appointments.GetByIdForUpdateAsync(request.Id, ct);
        if (appt is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Appointment not found");

        var result = appt.Transition(request.Status);
        if (result.IsFailure) return result;

        _appointments.Update(appt);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
