using ClinicManagement.Application.Abstractions.Data;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.Reference.Queries;

public class GetSpecializationsHandler : IRequestHandler<GetSpecializationsQuery, IEnumerable<SpecializationDto>>
{
    private readonly IUnitOfWork _uow;

    public GetSpecializationsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<SpecializationDto>> Handle(
        GetSpecializationsQuery request, CancellationToken cancellationToken)
    {
        var rows = await _uow.Reference.GetSpecializationsAsync(cancellationToken);
        return rows.Adapt<List<SpecializationDto>>();
    }
}
