using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.ChronicDiseases.Queries.GetChronicDisease;

public class GetChronicDiseaseQueryHandler : IRequestHandler<GetChronicDiseaseQuery, Result<ChronicDiseaseDto>>
{
    private readonly IRepository<ChronicDisease> _repository;
    private readonly ILogger<GetChronicDiseaseQueryHandler> _logger;

    public GetChronicDiseaseQueryHandler(
        IRepository<ChronicDisease> repository,
        ILogger<GetChronicDiseaseQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<ChronicDiseaseDto>> Handle(GetChronicDiseaseQuery request, CancellationToken cancellationToken)
    {
        var chronicDisease = await _repository.GetByIdAsync(request.Id, cancellationToken);
        
        if (chronicDisease == null)
        {
            _logger.LogWarning("Chronic disease {DiseaseId} not found", request.Id);
            return Result<ChronicDiseaseDto>.Fail(MessageCodes.Business.CHRONIC_DISEASE_NOT_FOUND);
        }

        var dto = chronicDisease.Adapt<ChronicDiseaseDto>();
        return Result<ChronicDiseaseDto>.Ok(dto);
    }
}
