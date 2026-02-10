using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.Specializations.Queries.GetSpecializations;

public record GetSpecializationsQuery : IRequest<Result<IEnumerable<SpecializationDto>>>;

public class GetSpecializationsQueryHandler : IRequestHandler<GetSpecializationsQuery, Result<IEnumerable<SpecializationDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetSpecializationsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<SpecializationDto>>> Handle(GetSpecializationsQuery request, CancellationToken cancellationToken)
    {
        var specializations = await _unitOfWork.Specializations.GetActiveAsync(cancellationToken);
        var specializationDtos = specializations.Adapt<IEnumerable<SpecializationDto>>();
        return Result<IEnumerable<SpecializationDto>>.Ok(specializationDtos);
    }
}