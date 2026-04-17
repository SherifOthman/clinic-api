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
        var visitType = await _uow.DoctorVisitTypes.GetByIdAsync(request.VisitTypeId, ct);
        if (visitType is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Visit type not found");

        var permission = await _permissions.CanManageVisitTypesByDoctorIdAsync(visitType.DoctorId, ct);
        if (!permission.IsAllowed)
            return Result.Failure(ErrorCodes.FORBIDDEN, permission.DeniedReason!);

        // Business rule: block removal if appointments exist
        var hasAppointments = await _uow.DoctorVisitTypes.HasAppointmentsAsync(request.VisitTypeId, ct);
        if (hasAppointments)
            return Result.Failure(ErrorCodes.CONFLICT, "Cannot remove a visit type that has existing appointments");

        _uow.DoctorVisitTypes.Remove(visitType);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
