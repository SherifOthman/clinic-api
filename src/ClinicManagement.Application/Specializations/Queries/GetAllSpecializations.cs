using ClinicManagement.Domain.Repositories;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Specializations.Queries;

public record GetAllSpecializationsQuery : IRequest<IEnumerable<SpecializationDto>>;

public record SpecializationDto(
    int Id,
    string NameEn,
    string NameAr,
    string? DescriptionEn,
    string? DescriptionAr
);

public class GetAllSpecializationsHandler : IRequestHandler<GetAllSpecializationsQuery, IEnumerable<SpecializationDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllSpecializationsHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<SpecializationDto>> Handle(GetAllSpecializationsQuery request, CancellationToken cancellationToken)
    {
        var specializations = await _unitOfWork.Specializations.GetAllAsync(cancellationToken);
        return specializations.Adapt<IEnumerable<SpecializationDto>>();
    }
}
