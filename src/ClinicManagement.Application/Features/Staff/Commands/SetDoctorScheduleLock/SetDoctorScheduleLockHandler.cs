using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class SetDoctorScheduleLockHandler : IRequestHandler<SetDoctorScheduleLockCommand, Result>
{
    private readonly IUnitOfWork _uow;
    public SetDoctorScheduleLockHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(SetDoctorScheduleLockCommand request, CancellationToken ct)
    {
        var doctorId = await _uow.DoctorProfiles.GetIdByStaffIdAsync(request.StaffId, ct);
        if (doctorId == Guid.Empty)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Doctor profile not found");

        var doctor = await _uow.DoctorProfiles.GetByIdAsync(doctorId, ct);
        if (doctor is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Doctor profile not found");

        doctor.CanSelfManageSchedule = request.CanSelfManage;
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
