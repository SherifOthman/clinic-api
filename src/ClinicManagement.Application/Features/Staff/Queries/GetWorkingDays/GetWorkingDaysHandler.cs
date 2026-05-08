using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Queries;

public class GetWorkingDaysHandler : IRequestHandler<GetWorkingDaysQuery, Result<List<WorkingDayDto>>>
{
    private readonly IDoctorInfoRepository     _doctorInfos;
    private readonly IDoctorScheduleRepository _schedules;

    public GetWorkingDaysHandler(IDoctorInfoRepository doctorInfos, IDoctorScheduleRepository schedules)
    {
        _doctorInfos = doctorInfos;
        _schedules   = schedules;
    }

    public async Task<Result<List<WorkingDayDto>>> Handle(
        GetWorkingDaysQuery request, CancellationToken cancellationToken)
    {
        var doctorInfoId = await _doctorInfos.GetIdByMemberIdAsync(request.StaffId, cancellationToken);
        if (doctorInfoId == Guid.Empty)
            return Result.Success(new List<WorkingDayDto>());

        var days = await _schedules.GetWorkingDaysByDoctorInfoIdAsync(doctorInfoId, cancellationToken);

        if (request.BranchId.HasValue)
            days = days.Where(d => d.BranchId == request.BranchId.Value).ToList();

        return Result.Success(days.Select(d => new WorkingDayDto(d.Day, d.StartTime, d.EndTime, d.IsAvailable, d.BranchId)).ToList());
    }
}
