using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

public class SetDoctorAppointmentTypeHandler : IRequestHandler<SetDoctorAppointmentTypeCommand, Result>
{
    private readonly IDoctorInfoRepository     _doctorInfos;
    private readonly IDoctorScheduleRepository _schedules;
    private readonly IUnitOfWork _uow;
    private readonly IPermissionService _permissions;

    public SetDoctorAppointmentTypeHandler(
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

    public async Task<Result> Handle(SetDoctorAppointmentTypeCommand request, CancellationToken ct)
    {
        var permission = await _permissions.CanManageVisitTypesAsync(request.MemberId, ct);
        if (!permission.IsAllowed)
            return Result.Failure(ErrorCodes.FORBIDDEN, permission.DeniedReason!);

        var doctorInfoId = await _doctorInfos.GetIdByMemberIdAsync(request.MemberId, ct);
        if (doctorInfoId == Guid.Empty)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Doctor not found");

        var schedule = await _schedules.GetScheduleAsync(doctorInfoId, request.BranchId, ct);
        if (schedule is null || !schedule.IsActive)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Doctor has no active schedule at this branch.");

        schedule.AppointmentType = request.AppointmentType;
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
