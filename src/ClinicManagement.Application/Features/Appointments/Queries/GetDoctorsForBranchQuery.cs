using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Features.Staff.QueryModels;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Queries;

public record GetDoctorsForBranchQuery(Guid BranchId) : IRequest<List<DoctorForBranchRow>>;

public class GetDoctorsForBranchHandler : IRequestHandler<GetDoctorsForBranchQuery, List<DoctorForBranchRow>>
{
    private readonly IUnitOfWork _uow;
    public GetDoctorsForBranchHandler(IUnitOfWork uow) => _uow = uow;

    public Task<List<DoctorForBranchRow>> Handle(GetDoctorsForBranchQuery request, CancellationToken ct)
        => _uow.DoctorSchedules.GetDoctorsForBranchAsync(request.BranchId, ct);
}
