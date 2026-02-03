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
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            DescriptionEn = request.DescriptionEn,
            DescriptionAr = request.DescriptionAr
        };

        await _unitOfWork.ChronicDiseases.AddAsync(chronicDisease, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Chronic disease {DiseaseName} created with ID {DiseaseId}", request.NameEn, chronicDisease.Id);

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
