using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.ChronicDiseases.Commands.DeleteChronicDisease;

public class DeleteChronicDiseaseCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; set; }
}

public class DeleteChronicDiseaseCommandHandler : IRequestHandler<DeleteChronicDiseaseCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DeleteChronicDiseaseCommandHandler> _logger;

    public DeleteChronicDiseaseCommandHandler(
        IApplicationDbContext context,
        ILogger<DeleteChronicDiseaseCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(DeleteChronicDiseaseCommand request, CancellationToken cancellationToken)
    {
        var chronicDisease = await _context.ChronicDiseases.FindAsync(new object[] { request.Id }, cancellationToken);
        
        if (chronicDisease == null)
        {
            _logger.LogWarning("Chronic disease {DiseaseId} not found for deletion", request.Id);
            return Result<Unit>.Fail(MessageCodes.Business.CHRONIC_DISEASE_NOT_FOUND);
        }

        _context.ChronicDiseases.Remove(chronicDisease);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Chronic disease {DiseaseId} deleted successfully", request.Id);

        return Result<Unit>.Ok(Unit.Value);
    }
}