using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Interfaces;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.ChronicDiseases.Queries.GetChronicDiseases;

public class GetChronicDiseasesQueryHandler : IRequestHandler<GetChronicDiseasesQuery, Result<List<ChronicDiseaseDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetChronicDiseasesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<ChronicDiseaseDto>>> Handle(GetChronicDiseasesQuery request, CancellationToken cancellationToken)
    {
        var chronicDiseases = await _unitOfWork.ChronicDiseases.GetActiveAsync(cancellationToken);
        var chronicDiseaseDtos = chronicDiseases.Adapt<List<ChronicDiseaseDto>>();
        
        return Result<List<ChronicDiseaseDto>>.Ok(chronicDiseaseDtos);
    }
}
