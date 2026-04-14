using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Reference.Queries;

public class GetChronicDiseasesQueryHandler : IRequestHandler<GetChronicDiseasesQuery, Result<List<ChronicDiseaseDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetChronicDiseasesQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<ChronicDiseaseDto>>> Handle(
        GetChronicDiseasesQuery request, CancellationToken cancellationToken)
    {
        var rows = await _uow.Reference.GetChronicDiseasesAsync(cancellationToken);
        return Result.Success(rows.Select(r => new ChronicDiseaseDto { Id = r.Id, NameEn = r.NameEn, NameAr = r.NameAr }).ToList());
    }
}
