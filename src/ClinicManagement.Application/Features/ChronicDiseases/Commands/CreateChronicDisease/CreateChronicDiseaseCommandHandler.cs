using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Entities;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.ChronicDiseases.Commands.CreateChronicDisease;

public class CreateChronicDiseaseCommandHandler : IRequestHandler<CreateChronicDiseaseCommand, Result<ChronicDiseaseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateChronicDiseaseCommandHandler> _logger;

    public CreateChronicDiseaseCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CreateChronicDiseaseCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ChronicDiseaseDto>> Handle(CreateChronicDiseaseCommand request, CancellationToken cancellationToken)
    {
        var chronicDisease = new ChronicDisease
        {
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive
        };

        await _unitOfWork.ChronicDiseases.AddAsync(chronicDisease, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Chronic disease {DiseaseName} created with ID {DiseaseId}", request.Name, chronicDisease.Id);

        var dto = chronicDisease.Adapt<ChronicDiseaseDto>();
        return Result<ChronicDiseaseDto>.Ok(dto);
    }
}
