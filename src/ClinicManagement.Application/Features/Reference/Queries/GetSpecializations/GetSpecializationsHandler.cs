using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Reference.Queries;

public class GetSpecializationsHandler : IRequestHandler<GetSpecializationsQuery, Result<List<SpecializationDto>>>
{
    private readonly IReferenceRepository _reference;

    public GetSpecializationsHandler(IReferenceRepository reference) => _reference = reference;

    public async Task<Result<List<SpecializationDto>>> Handle(
        GetSpecializationsQuery request, CancellationToken cancellationToken)
    {
        var rows = await _reference.GetSpecializationsAsync(cancellationToken);
        return Result.Success(rows.Select(r => new SpecializationDto(r.Id, r.NameEn, r.NameAr, r.DescriptionEn, r.DescriptionAr)).ToList());
    }
}
