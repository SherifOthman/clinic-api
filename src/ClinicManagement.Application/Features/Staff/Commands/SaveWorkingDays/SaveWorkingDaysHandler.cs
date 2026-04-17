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
        var doctorProfileId = await _uow.DoctorProfiles.GetIdByStaffIdAsync(request.StaffId, cancellationToken);
        if (doctorProfileId == Guid.Empty)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Doctor profile not found for this staff member");

        var permission = await _permissions.CanManageScheduleAsync(request.StaffId, cancellationToken);
        if (!permission.IsAllowed)
            return Result.Failure(ErrorCodes.FORBIDDEN, permission.DeniedReason!);

        var existing = await _uow.WorkingDays.GetEntitiesByDoctorProfileIdAsync(doctorProfileId, cancellationToken);
        var branchExisting = existing.Where(d => d.ClinicBranchId == request.BranchId).ToList();
        _uow.WorkingDays.RemoveRange(branchExisting);

        foreach (var day in request.Days)
        {
            _uow.WorkingDays.Add(new DoctorWorkingDay
            {
                DoctorId       = doctorProfileId,
                ClinicBranchId = request.BranchId,
                Day            = (DayOfWeek)day.Day,
                StartTime      = TimeOnly.Parse(day.StartTime),
                EndTime        = TimeOnly.Parse(day.EndTime),
                IsAvailable    = day.IsAvailable,
            });
        }

        await _uow.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
