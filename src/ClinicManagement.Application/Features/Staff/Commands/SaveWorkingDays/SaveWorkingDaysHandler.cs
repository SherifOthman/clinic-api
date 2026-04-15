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
    private readonly ICurrentUserService _currentUser;

    public SaveWorkingDaysHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(SaveWorkingDaysCommand request, CancellationToken cancellationToken)
    {
        var doctorProfileId = await _uow.DoctorProfiles.GetIdByStaffIdAsync(request.StaffId, cancellationToken);
        if (doctorProfileId == Guid.Empty)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Doctor profile not found for this staff member");

        // Authorization: clinic owner can edit any doctor; doctor can only edit own if not locked
        var isOwner = _currentUser.Roles.Contains(UserRoles.ClinicOwner);
        if (!isOwner)
        {
            var staff = await _uow.Staff.GetByUserIdAsync(_currentUser.GetRequiredUserId(), cancellationToken);
            if (staff?.Id != request.StaffId)
                return Result.Failure(ErrorCodes.FORBIDDEN, "You can only manage your own schedule");

            var doctor = await _uow.DoctorProfiles.GetByIdAsync(doctorProfileId, cancellationToken);
            if (doctor is not null && !doctor.CanSelfManageSchedule)
                return Result.Failure(ErrorCodes.FORBIDDEN, "Schedule management is locked by the clinic owner");
        }

        // Replace working days for this specific branch only
        var existing = await _uow.WorkingDays.GetEntitiesByDoctorProfileIdAsync(doctorProfileId, cancellationToken);
        var branchExisting = existing.Where(d => d.ClinicBranchId == request.BranchId).ToList();
        _uow.WorkingDays.RemoveRange(branchExisting);

        foreach (var day in request.Days)
        {
            _uow.WorkingDays.Add(new DoctorWorkingDay
            {
                DoctorId              = doctorProfileId,
                ClinicBranchId        = request.BranchId,
                Day                   = (DayOfWeek)day.Day,
                StartTime             = TimeOnly.Parse(day.StartTime),
                EndTime               = TimeOnly.Parse(day.EndTime),
                IsAvailable           = day.IsAvailable,
                MaxAppointmentsPerDay = day.MaxAppointmentsPerDay,
            });
        }

        await _uow.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
