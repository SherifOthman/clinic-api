using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class SetDoctorScheduleLockHandler : IRequestHandler<SetDoctorScheduleLockCommand, Result>
{
    private readonly IDoctorInfoRepository _doctorInfos;
    private readonly IUnitOfWork           _uow;

    public SetDoctorScheduleLockHandler(IDoctorInfoRepository doctorInfos, IUnitOfWork uow)
    {
        _doctorInfos = doctorInfos;
        _uow         = uow;
    }

    public async Task<Result> Handle(SetDoctorScheduleLockCommand request, CancellationToken ct)
    {
        var doctorInfoId = await _doctorInfos.GetIdByMemberIdAsync(request.StaffId, ct);
        if (doctorInfoId == Guid.Empty)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Doctor profile not found");

        var doctorInfo = await _doctorInfos.GetByIdAsync(doctorInfoId, ct);
        if (doctorInfo is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Doctor profile not found");

        doctorInfo.CanSelfManageSchedule = request.CanSelfManage;
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
