using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Queries;

public class GetDoctorVisitTypesHandler : IRequestHandler<GetDoctorVisitTypesQuery, Result<List<DoctorVisitTypeDto>>>
{
    private readonly IUnitOfWork _uow;
    public GetDoctorVisitTypesHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<DoctorVisitTypeDto>>> Handle(GetDoctorVisitTypesQuery request, CancellationToken ct)
    {
        var doctorInfoId = await _uow.DoctorInfos.GetIdByMemberIdAsync(request.StaffId, ct);
        if (doctorInfoId == Guid.Empty)
            return Result.Failure<List<DoctorVisitTypeDto>>(ErrorCodes.NOT_FOUND, "Doctor profile not found");

        var schedule = await _uow.DoctorSchedules.GetScheduleAsync(doctorInfoId, request.BranchId, ct);
        if (schedule is null) return Result.Success(new List<DoctorVisitTypeDto>());

        var items = await _uow.DoctorSchedules.GetVisitTypesByScheduleAsync(schedule.Id, ct);
        var dtos  = items.Select(v => new DoctorVisitTypeDto(v.Id, v.Name, v.Price, v.IsActive)).ToList();
        return Result.Success(dtos);
    }
}
