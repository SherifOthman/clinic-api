using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.ChronicDiseases.Commands.UpdateChronicDisease;

public class UpdateChronicDiseaseCommand : IRequest<Result<ChronicDiseaseDto>>
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
}

public class UpdateChronicDiseaseCommandHandler : IRequestHandler<UpdateChronicDiseaseCommand, Result<ChronicDiseaseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateChronicDiseaseCommandHandler> _logger;

    public UpdateChronicDiseaseCommandHandler(
        IApplicationDbContext context,
        ILogger<UpdateChronicDiseaseCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<ChronicDiseaseDto>> Handle(UpdateChronicDiseaseCommand request, CancellationToken cancellationToken)
    {
        var chronicDisease = await _context.ChronicDiseases.FindAsync(new object[] { request.Id }, cancellationToken);
        
        if (chronicDisease == null)
        {
            _logger.LogWarning("Chronic disease {DiseaseId} not found for update", request.Id);
            return Result<ChronicDiseaseDto>.Fail(MessageCodes.Business.CHRONIC_DISEASE_NOT_FOUND);
        }

        chronicDisease.NameEn = request.NameEn;
        chronicDisease.NameAr = request.NameAr;
        chronicDisease.DescriptionEn = request.DescriptionEn;
        chronicDisease.DescriptionAr = request.DescriptionAr;

        _context.ChronicDiseases.Update(chronicDisease);
        await _context.SaveChangesAsync(cancellationToken);

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