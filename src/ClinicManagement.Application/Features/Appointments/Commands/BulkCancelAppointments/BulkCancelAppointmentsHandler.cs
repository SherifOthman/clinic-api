using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

public class BulkCancelAppointmentsHandler : IRequestHandler<BulkCancelAppointmentsCommand, Result<int>>
{
    private readonly IAppointmentRepository _appointments;
    private readonly IUnitOfWork _uow;

    public BulkCancelAppointmentsHandler(IAppointmentRepository appointments, IUnitOfWork uow)
    {
        _appointments = appointments;
        _uow          = uow;
    }

    public async Task<Result<int>> Handle(BulkCancelAppointmentsCommand request, CancellationToken ct)
    {
        var appointments = await _appointments.GetByDoctorAndDateForUpdateAsync(
            request.DoctorInfoId, request.Date, ct);

        var cancellable = appointments.Where(a =>
            a.BranchId == request.BranchId &&
            (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Waiting))
            .ToList();

        foreach (var appt in cancellable)
            appt.ForceCancel();

        await _uow.SaveChangesAsync(ct);
        return Result.Success(cancellable.Count);
    }
}
