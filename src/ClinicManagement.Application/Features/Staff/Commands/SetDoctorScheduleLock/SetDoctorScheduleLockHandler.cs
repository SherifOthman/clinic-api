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
        var doctorInfoId = await _uow.DoctorInfos.GetIdByMemberIdAsync(request.StaffId, ct);
        if (doctorInfoId == Guid.Empty)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Doctor profile not found");

        var doctorInfo = await _uow.DoctorInfos.GetByIdAsync(doctorInfoId, ct);
        if (doctorInfo is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Doctor profile not found");

        doctorInfo.CanSelfManageSchedule = request.CanSelfManage;
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
