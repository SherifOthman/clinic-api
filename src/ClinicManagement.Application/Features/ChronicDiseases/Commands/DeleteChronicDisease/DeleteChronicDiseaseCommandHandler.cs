using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.ChronicDiseases.Commands.DeleteChronicDisease;

public class DeleteChronicDiseaseCommandHandler : IRequestHandler<DeleteChronicDiseaseCommand, Result<Unit>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteChronicDiseaseCommandHandler> _logger;

    public DeleteChronicDiseaseCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DeleteChronicDiseaseCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(DeleteChronicDiseaseCommand request, CancellationToken cancellationToken)
    {
        var chronicDisease = await _unitOfWork.ChronicDiseases.GetByIdAsync(request.Id, cancellationToken);
        
        if (chronicDisease == null)
        {
            _logger.LogWarning("Chronic disease {DiseaseId} not found for deletion", request.Id);
            return Result<Unit>.Fail(MessageCodes.Business.CHRONIC_DISEASE_NOT_FOUND);
        }

        _unitOfWork.ChronicDiseases.Delete(chronicDisease);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Chronic disease {DiseaseId} deleted successfully", request.Id);

        return Result<Unit>.Ok(Unit.Value);
    }
}
