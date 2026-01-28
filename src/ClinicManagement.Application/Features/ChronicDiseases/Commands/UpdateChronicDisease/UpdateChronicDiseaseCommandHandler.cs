using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Entities;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.ChronicDiseases.Commands.UpdateChronicDisease;

public class UpdateChronicDiseaseCommandHandler : IRequestHandler<UpdateChronicDiseaseCommand, Result<ChronicDiseaseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateChronicDiseaseCommandHandler> _logger;

    public UpdateChronicDiseaseCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdateChronicDiseaseCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ChronicDiseaseDto>> Handle(UpdateChronicDiseaseCommand request, CancellationToken cancellationToken)
    {
        var chronicDisease = await _unitOfWork.ChronicDiseases.GetByIdAsync(request.Id, cancellationToken);
        
        if (chronicDisease == null)
        {
            _logger.LogWarning("Chronic disease {DiseaseId} not found for update", request.Id);
            return Result<ChronicDiseaseDto>.Fail(ApplicationErrors.Business.CHRONIC_DISEASE_NOT_FOUND);
        }

        chronicDisease.Name = request.Name;
        chronicDisease.Description = request.Description;
        chronicDisease.IsActive = request.IsActive;

        await _unitOfWork.ChronicDiseases.UpdateAsync(chronicDisease, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Chronic disease {DiseaseId} updated successfully", request.Id);

        var dto = chronicDisease.Adapt<ChronicDiseaseDto>();
        return Result<ChronicDiseaseDto>.Ok(dto);
    }
}
