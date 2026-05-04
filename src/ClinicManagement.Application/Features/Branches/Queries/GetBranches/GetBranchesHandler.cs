using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Branches.Queries;

public class GetBranchesHandler : IRequestHandler<GetBranchesQuery, Result<List<BranchDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetBranchesHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<BranchDto>>> Handle(GetBranchesQuery request, CancellationToken cancellationToken)
    {
        var branches = await _uow.Branches.GetProjectedListAsync(cancellationToken);
        var dtos = branches.Select(b => new BranchDto(
            b.Id, b.Name, b.AddressLine,
            b.StateGeonameId, b.CityGeonameId,
            b.IsMainBranch, b.IsActive,
            b.PhoneNumbers.Select(p => new BranchPhoneDto(p.PhoneNumber, p.Label)).ToList()
        )).ToList();
        return Result.Success(dtos);
    }
}
