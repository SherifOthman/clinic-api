using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

/// <summary>
/// Marks an appointment as paid by creating a placeholder InvoiceId.
/// A full invoicing system would create a real Invoice entity here.
/// For now we use a sentinel GUID to indicate "paid" state.
/// </summary>
public record MarkAppointmentPaidCommand(Guid Id) : IRequest<Result>;

public class MarkAppointmentPaidHandler : IRequestHandler<MarkAppointmentPaidCommand, Result>
{
    private readonly IAppointmentRepository _appointments;
    private readonly IUnitOfWork _uow;

    public MarkAppointmentPaidHandler(IAppointmentRepository appointments, IUnitOfWork uow)
    {
        _appointments = appointments;
        _uow          = uow;
    }

    public async Task<Result> Handle(MarkAppointmentPaidCommand request, CancellationToken ct)
    {
        var appointment = await _appointments.GetByIdForUpdateAsync(request.Id, ct);
        if (appointment is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Appointment not found");

        if (appointment.InvoiceId.HasValue)
            return Result.Failure(ErrorCodes.ALREADY_EXISTS, "Appointment is already marked as paid");

        appointment.InvoiceId = Guid.NewGuid();
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}

/// <summary>Removes the paid marker (refund).</summary>
public record RefundAppointmentCommand(Guid Id) : IRequest<Result>;

public class RefundAppointmentHandler : IRequestHandler<RefundAppointmentCommand, Result>
{
    private readonly IAppointmentRepository _appointments;
    private readonly IUnitOfWork _uow;

    public RefundAppointmentHandler(IAppointmentRepository appointments, IUnitOfWork uow)
    {
        _appointments = appointments;
        _uow          = uow;
    }

    public async Task<Result> Handle(RefundAppointmentCommand request, CancellationToken ct)
    {
        var appointment = await _appointments.GetByIdForUpdateAsync(request.Id, ct);
        if (appointment is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Appointment not found");

        if (!appointment.InvoiceId.HasValue)
            return Result.Failure(ErrorCodes.OPERATION_NOT_ALLOWED, "Appointment has not been paid");

        appointment.InvoiceId = null;
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
