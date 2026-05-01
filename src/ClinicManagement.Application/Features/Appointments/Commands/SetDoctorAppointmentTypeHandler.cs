using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

public class SetDoctorAppointmentTypeHandler : IRequestHandler<SetDoctorAppointmentTypeCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public SetDoctorAppointmentTypeHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow          = uow;
        _currentUser  = currentUser;
    }

    public async Task<Result> Handle(SetDoctorAppointmentTypeCommand request, CancellationToken ct)
    {
        var clinicId = _currentUser.GetRequiredClinicId();

        // Load the ClinicMember with DoctorInfo — must belong to this clinic
        var member = await _uow.Members.GetByIdWithDoctorInfoAsync(request.MemberId, ct);
        if (member is null || member.ClinicId != clinicId || member.DoctorInfo is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Doctor not found");

        // Allow: clinic owner OR the doctor themselves (when CanSelfManageSchedule is true)
        var isOwner   = _currentUser.Roles.Contains(UserRoles.ClinicOwner);
        var isSelf    = member.UserId.HasValue && member.UserId == _currentUser.UserId;
        var canManage = isOwner || (isSelf && member.DoctorInfo.CanSelfManageSchedule);

        if (!canManage)
            return Result.Failure(ErrorCodes.FORBIDDEN,
                "You are not allowed to change this doctor's appointment type.");

        // Load the branch schedule — must exist and be active
        var schedule = await _uow.DoctorSchedules.GetScheduleAsync(
            member.DoctorInfo.Id, request.BranchId, ct);

        if (schedule is null || !schedule.IsActive)
            return Result.Failure(ErrorCodes.NOT_FOUND,
                "Doctor has no active schedule at this branch.");

        schedule.AppointmentType = request.AppointmentType;
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
