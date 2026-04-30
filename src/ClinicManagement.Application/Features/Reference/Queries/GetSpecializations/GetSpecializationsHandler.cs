using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Reference.Queries;

public class GetSpecializationsHandler : IRequestHandler<GetSpecializationsQuery, Result<List<SpecializationDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetSpecializationsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<SpecializationDto>>> Handle(
        GetSpecializationsQuery request, CancellationToken cancellationToken)
    {
        var rows = await _uow.Reference.GetSpecializationsAsync(cancellationToken);
        var list = rows.Select(r => new SpecializationDto(r.Id, r.NameEn, r.NameAr, r.DescriptionEn, r.DescriptionAr))
                       .ToList();

        return Result.Success(list);
    }
}
