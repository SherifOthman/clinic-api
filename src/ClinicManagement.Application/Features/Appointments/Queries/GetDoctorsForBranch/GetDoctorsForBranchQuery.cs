using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Features.Staff.QueryModels;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Queries;

public record GetDoctorsForBranchQuery(Guid BranchId) : IRequest<Result<List<DoctorForBranchRow>>>;

public class GetDoctorsForBranchHandler : IRequestHandler<GetDoctorsForBranchQuery, Result<List<DoctorForBranchRow>>>
{
    private readonly IDoctorScheduleRepository _schedules;

    public GetDoctorsForBranchHandler(IDoctorScheduleRepository schedules) => _schedules = schedules;

    public async Task<Result<List<DoctorForBranchRow>>> Handle(GetDoctorsForBranchQuery request, CancellationToken ct)
    {
        var rows = await _schedules.GetDoctorsForBranchAsync(request.BranchId, ct);
        return Result.Success(rows);
    }
}
