using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

public record UpdateAppointmentStatusCommand(Guid Id, AppointmentStatus Status) : IRequest<Result>;

public class UpdateAppointmentStatusHandler : IRequestHandler<UpdateAppointmentStatusCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public UpdateAppointmentStatusHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(UpdateAppointmentStatusCommand request, CancellationToken ct)
    {
        var appt = await _uow.Appointments.GetByIdForUpdateAsync(request.Id, ct);
        if (appt is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Appointment not found");

        // Transition() owns all allowed-state logic — it lives in the domain, not here.
        var result = appt.Transition(request.Status);
        if (result.IsFailure) return result;

        _uow.Appointments.Update(appt);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
