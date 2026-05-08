using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class UpsertDoctorVisitTypeHandler : IRequestHandler<UpsertDoctorVisitTypeCommand, Result<Guid>>
{
    private readonly IDoctorInfoRepository     _doctorInfos;
    private readonly IDoctorScheduleRepository _schedules;
    private readonly IUnitOfWork               _uow;
    private readonly IPermissionService        _permissions;

    public UpsertDoctorVisitTypeHandler(
        IDoctorInfoRepository doctorInfos,
        IDoctorScheduleRepository schedules,
        IUnitOfWork uow,
        IPermissionService permissions)
    {
        _doctorInfos = doctorInfos;
        _schedules   = schedules;
        _uow         = uow;
        _permissions = permissions;
    }

    public async Task<Result<Guid>> Handle(UpsertDoctorVisitTypeCommand request, CancellationToken ct)
    {
        var permission = await _permissions.CanManageVisitTypesAsync(request.StaffId, ct);
        if (!permission.IsAllowed)
            return Result.Failure<Guid>(ErrorCodes.FORBIDDEN, permission.DeniedReason!);

        var doctorInfoId = await _doctorInfos.GetIdByMemberIdAsync(request.StaffId, ct);
        if (doctorInfoId == Guid.Empty)
            return Result.Failure<Guid>(ErrorCodes.NOT_FOUND, "Doctor profile not found");

        var schedule = await _schedules.GetOrCreateScheduleAsync(doctorInfoId, request.BranchId, ct);

        if (request.VisitTypeId.HasValue)
        {
            var existing = await _schedules.GetVisitTypeByIdAsync(request.VisitTypeId.Value, ct);
            if (existing is null || existing.DoctorBranchScheduleId != schedule.Id)
                return Result.Failure<Guid>(ErrorCodes.NOT_FOUND, "Visit type not found");

            existing.Name     = request.Name;
            existing.Price    = request.Price;
            existing.IsActive = request.IsActive;
            await _uow.SaveChangesAsync(ct);
            return Result.Success(existing.Id);
        }

        var visitType = new VisitType
        {
            DoctorBranchScheduleId = schedule.Id,
            Name     = request.Name,
            Price    = request.Price,
            IsActive = request.IsActive,
        };
        _schedules.AddVisitType(visitType);
        await _uow.SaveChangesAsync(ct);
        return Result.Success(visitType.Id);
    }
}
