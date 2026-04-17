using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class SaveWorkingDaysHandler : IRequestHandler<SaveWorkingDaysCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly IPermissionService _permissions;

    public SaveWorkingDaysHandler(IUnitOfWork uow, IPermissionService permissions)
    {
        _uow         = uow;
        _permissions = permissions;
    }

    public async Task<Result> Handle(SaveWorkingDaysCommand request, CancellationToken cancellationToken)
    {
        var permission = await _permissions.CanManageScheduleAsync(request.StaffId, cancellationToken);
        if (!permission.IsAllowed)
            return Result.Failure(ErrorCodes.FORBIDDEN, permission.DeniedReason!);

        var doctorInfoId = await _uow.DoctorInfos.GetIdByMemberIdAsync(request.StaffId, cancellationToken);
        if (doctorInfoId == Guid.Empty)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Doctor profile not found for this staff member");

        var schedule = await _uow.DoctorSchedules.GetOrCreateScheduleAsync(doctorInfoId, request.BranchId, cancellationToken);
        var existing = await _uow.DoctorSchedules.GetWorkingDayEntitiesAsync(schedule.Id, cancellationToken);
        _uow.DoctorSchedules.RemoveWorkingDays(existing);

        foreach (var day in request.Days)
        {
            _uow.DoctorSchedules.AddWorkingDay(new WorkingDay
            {
                DoctorBranchScheduleId = schedule.Id,
                Day         = (DayOfWeek)day.Day,
                StartTime   = TimeOnly.Parse(day.StartTime),
                EndTime     = TimeOnly.Parse(day.EndTime),
                IsAvailable = day.IsAvailable,
            });
        }

        await _uow.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
