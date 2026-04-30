using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Features.Staff.QueryModels;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Queries;

public record GetDoctorsForBranchQuery(Guid BranchId) : IRequest<Result<List<DoctorForBranchRow>>>;

public class GetDoctorsForBranchHandler : IRequestHandler<GetDoctorsForBranchQuery, Result<List<DoctorForBranchRow>>>
{
    private readonly IUnitOfWork _uow;
    public GetDoctorsForBranchHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<DoctorForBranchRow>>> Handle(GetDoctorsForBranchQuery request, CancellationToken ct)
    {
        var rows = await _uow.DoctorSchedules.GetDoctorsForBranchAsync(request.BranchId, ct);
        return Result.Success(rows);
    }
}
