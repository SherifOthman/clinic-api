using ClinicManagement.Application.Abstractions.Data;
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
    private readonly IUnitOfWork _uow;
    public MarkAppointmentPaidHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(MarkAppointmentPaidCommand request, CancellationToken ct)
    {
        var appointment = await _uow.Appointments.GetByIdForUpdateAsync(request.Id, ct);
        if (appointment is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Appointment not found");

        if (appointment.InvoiceId.HasValue)
            return Result.Failure(ErrorCodes.ALREADY_EXISTS, "Appointment is already marked as paid");

        // Use a non-empty GUID as a "paid" marker until a full invoicing system is built
        appointment.InvoiceId = Guid.NewGuid();
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}

/// <summary>Removes the paid marker (refund).</summary>
public record RefundAppointmentCommand(Guid Id) : IRequest<Result>;

public class RefundAppointmentHandler : IRequestHandler<RefundAppointmentCommand, Result>
{
    private readonly IUnitOfWork _uow;
    public RefundAppointmentHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(RefundAppointmentCommand request, CancellationToken ct)
    {
        var appointment = await _uow.Appointments.GetByIdForUpdateAsync(request.Id, ct);
        if (appointment is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Appointment not found");

        if (!appointment.InvoiceId.HasValue)
            return Result.Failure(ErrorCodes.OPERATION_NOT_ALLOWED, "Appointment has not been paid");

        appointment.InvoiceId = null;
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
