using ClinicManagement.Domain.Common.Constants;
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
            return Result<ChronicDiseaseDto>.Fail(MessageCodes.Business.CHRONIC_DISEASE_NOT_FOUND);
        }

        chronicDisease.NameEn = request.NameEn;
        chronicDisease.NameAr = request.NameAr;
        chronicDisease.DescriptionEn = request.DescriptionEn;
        chronicDisease.DescriptionAr = request.DescriptionAr;

        _unitOfWork.ChronicDiseases.Update(chronicDisease);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Chronic disease {DiseaseId} updated successfully", request.Id);

        var dto = new ChronicDiseaseDto
        {
            Id = chronicDisease.Id,
            NameEn = chronicDisease.NameEn,
            NameAr = chronicDisease.NameAr,
            DescriptionEn = chronicDisease.DescriptionEn,
            DescriptionAr = chronicDisease.DescriptionAr,
            Name = chronicDisease.NameEn, // Default to English
            Description = chronicDisease.DescriptionEn
        };
        
        return Result<ChronicDiseaseDto>.Ok(dto);
    }
}
