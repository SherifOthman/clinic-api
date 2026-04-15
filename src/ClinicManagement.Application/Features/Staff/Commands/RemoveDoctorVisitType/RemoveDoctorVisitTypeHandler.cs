using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class RemoveDoctorVisitTypeHandler : IRequestHandler<RemoveDoctorVisitTypeCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public RemoveDoctorVisitTypeHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(RemoveDoctorVisitTypeCommand request, CancellationToken ct)
    {
        var visitType = await _uow.DoctorVisitTypes.GetByIdAsync(request.VisitTypeId, ct);
        if (visitType is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Visit type not found");

        // Authorization
        var isOwner = _currentUser.Roles.Contains(UserRoles.ClinicOwner);
        if (!isOwner)
        {
            var staff = await _uow.Staff.GetByUserIdAsync(_currentUser.GetRequiredUserId(), ct);
            var doctorId = staff is not null ? await _uow.DoctorProfiles.GetIdByStaffIdAsync(staff.Id, ct) : Guid.Empty;
            if (doctorId != visitType.DoctorId)
                return Result.Failure(ErrorCodes.FORBIDDEN, "You can only remove your own visit types");

            var doctor = await _uow.DoctorProfiles.GetByIdAsync(visitType.DoctorId, ct);
            if (doctor is not null && !doctor.CanSelfManageSchedule)
                return Result.Failure(ErrorCodes.FORBIDDEN, "Schedule management is locked by the clinic owner");
        }

        // Block removal if appointments exist
        var hasAppointments = await _uow.DoctorVisitTypes.HasAppointmentsAsync(request.VisitTypeId, ct);
        if (hasAppointments)
            return Result.Failure(ErrorCodes.CONFLICT, "Cannot remove a visit type that has existing appointments");

        _uow.DoctorVisitTypes.Remove(visitType);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
