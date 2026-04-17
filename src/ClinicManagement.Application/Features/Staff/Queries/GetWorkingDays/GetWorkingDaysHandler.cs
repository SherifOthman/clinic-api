using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Queries;

public class GetWorkingDaysHandler : IRequestHandler<GetWorkingDaysQuery, Result<List<WorkingDayDto>>>
{
    private readonly IUnitOfWork _uow;
    public GetWorkingDaysHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<WorkingDayDto>>> Handle(
        GetWorkingDaysQuery request, CancellationToken cancellationToken)
    {
        // Try new model first
        var doctorInfoId = await _uow.DoctorInfos.GetIdByMemberIdAsync(request.StaffId, cancellationToken);
        if (doctorInfoId != Guid.Empty)
        {
            var days = await _uow.DoctorSchedules.GetWorkingDaysByDoctorInfoIdAsync(doctorInfoId, cancellationToken);
            if (request.BranchId.HasValue)
                days = days.Where(d => d.BranchId == request.BranchId.Value).ToList();
            return Result.Success(days.Select(d => new WorkingDayDto(d.Day, d.StartTime, d.EndTime, d.IsAvailable, d.BranchId)).ToList());
        }

        // Fall back to old model
        var oldDoctorId = await _uow.DoctorProfiles.GetIdByStaffIdAsync(request.StaffId, cancellationToken);
        if (oldDoctorId == Guid.Empty)
            return Result.Success(new List<WorkingDayDto>());

        var oldDays = await _uow.WorkingDays.GetByDoctorProfileIdAsync(oldDoctorId, cancellationToken);
        if (request.BranchId.HasValue)
            oldDays = oldDays.Where(d => d.BranchId == request.BranchId.Value).ToList();

        return Result.Success(oldDays.Select(d => new WorkingDayDto(d.Day, d.StartTime, d.EndTime, d.IsAvailable, d.BranchId)).ToList());
    }
}
