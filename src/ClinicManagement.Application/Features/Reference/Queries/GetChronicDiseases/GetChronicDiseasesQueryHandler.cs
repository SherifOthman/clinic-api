using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Reference.Queries;

public class GetChronicDiseasesQueryHandler : IRequestHandler<GetChronicDiseasesQuery, Result<List<ChronicDiseaseDto>>>
{
    private readonly IReferenceRepository _reference;

    public GetChronicDiseasesQueryHandler(IReferenceRepository reference) => _reference = reference;

    public async Task<Result<List<ChronicDiseaseDto>>> Handle(
        GetChronicDiseasesQuery request, CancellationToken cancellationToken)
    {
        var rows = await _reference.GetChronicDiseasesAsync(cancellationToken);
        return Result.Success(rows.Select(r => new ChronicDiseaseDto { Id = r.Id, NameEn = r.NameEn, NameAr = r.NameAr }).ToList());
    }
}
