using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.ChronicDiseases.Commands.CreateChronicDisease;

public class CreateChronicDiseaseCommand : IRequest<Result<ChronicDiseaseDto>>
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
}

public class CreateChronicDiseaseCommandHandler : IRequestHandler<CreateChronicDiseaseCommand, Result<ChronicDiseaseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateChronicDiseaseCommandHandler> _logger;

    public CreateChronicDiseaseCommandHandler(
        IApplicationDbContext context,
        ILogger<CreateChronicDiseaseCommandHandler> logger)
    {
        _context = context;
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

        _context.ChronicDiseases.Add(chronicDisease);
        await _context.SaveChangesAsync(cancellationToken);

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