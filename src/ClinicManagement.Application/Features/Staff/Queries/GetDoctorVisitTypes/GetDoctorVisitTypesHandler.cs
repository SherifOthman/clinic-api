using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Queries;

public class GetDoctorVisitTypesHandler : IRequestHandler<GetDoctorVisitTypesQuery, Result<List<DoctorVisitTypeDto>>>
{
    private readonly IDoctorInfoRepository     _doctorInfos;
    private readonly IDoctorScheduleRepository _schedules;

    public GetDoctorVisitTypesHandler(IDoctorInfoRepository doctorInfos, IDoctorScheduleRepository schedules)
    {
        _doctorInfos = doctorInfos;
        _schedules   = schedules;
    }

    public async Task<Result<List<DoctorVisitTypeDto>>> Handle(GetDoctorVisitTypesQuery request, CancellationToken ct)
    {
        var doctorInfoId = await _doctorInfos.GetIdByMemberIdAsync(request.StaffId, ct);
        if (doctorInfoId == Guid.Empty)
            return Result.Failure<List<DoctorVisitTypeDto>>(ErrorCodes.NOT_FOUND, "Doctor profile not found");

        var schedule = await _schedules.GetScheduleAsync(doctorInfoId, request.BranchId, ct);
        if (schedule is null) return Result.Success(new List<DoctorVisitTypeDto>());

        var items = await _schedules.GetVisitTypesByScheduleAsync(schedule.Id, ct);
        var dtos  = items.Select(v => new DoctorVisitTypeDto(v.Id, v.Name, v.Price, v.IsActive)).ToList();
        return Result.Success(dtos);
    }
}
