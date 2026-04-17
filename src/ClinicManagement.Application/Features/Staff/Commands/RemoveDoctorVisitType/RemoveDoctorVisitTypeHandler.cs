using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class RemoveDoctorVisitTypeHandler : IRequestHandler<RemoveDoctorVisitTypeCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly IPermissionService _permissions;

    public RemoveDoctorVisitTypeHandler(IUnitOfWork uow, IPermissionService permissions)
    {
        _uow         = uow;
        _permissions = permissions;
    }

    public async Task<Result> Handle(RemoveDoctorVisitTypeCommand request, CancellationToken ct)
    {
        // Try new model first
        var newVisitType = await _uow.DoctorSchedules.GetVisitTypeByIdAsync(request.VisitTypeId, ct);
        if (newVisitType is not null)
        {
            var permission = await _permissions.CanManageVisitTypesByDoctorIdAsync(newVisitType.Schedule.DoctorInfoId, ct);
            if (!permission.IsAllowed)
                return Result.Failure(ErrorCodes.FORBIDDEN, permission.DeniedReason!);

            var hasAppointments = await _uow.DoctorSchedules.VisitTypeHasAppointmentsAsync(request.VisitTypeId, ct);
            if (hasAppointments)
                return Result.Failure(ErrorCodes.CONFLICT, "Cannot remove a visit type that has existing appointments");

            _uow.DoctorSchedules.RemoveVisitType(newVisitType);
            await _uow.SaveChangesAsync(ct);
            return Result.Success();
        }

        // Fall back to old model
        var visitType = await _uow.DoctorVisitTypes.GetByIdAsync(request.VisitTypeId, ct);
        if (visitType is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Visit type not found");

        var oldPermission = await _permissions.CanManageVisitTypesByDoctorIdAsync(visitType.DoctorId, ct);
        if (!oldPermission.IsAllowed)
            return Result.Failure(ErrorCodes.FORBIDDEN, oldPermission.DeniedReason!);

        var oldHasAppointments = await _uow.DoctorVisitTypes.HasAppointmentsAsync(request.VisitTypeId, ct);
        if (oldHasAppointments)
            return Result.Failure(ErrorCodes.CONFLICT, "Cannot remove a visit type that has existing appointments");

        _uow.DoctorVisitTypes.Remove(visitType);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
