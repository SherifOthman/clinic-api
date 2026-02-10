using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Patients.Commands.RemoveChronicDisease;

public record RemoveChronicDiseaseCommand(
    Guid PatientId,
    Guid ChronicDiseaseId
) : IRequest<Result>;

public class RemoveChronicDiseaseCommandHandler : IRequestHandler<RemoveChronicDiseaseCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RemoveChronicDiseaseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(RemoveChronicDiseaseCommand request, CancellationToken cancellationToken)
    {
        var PatientChronicDisease = await _context.PatientChronicDiseases
            .FirstOrDefaultAsync(pcd => pcd.PatientId == request.PatientId && pcd.ChronicDiseaseId == request.ChronicDiseaseId, cancellationToken);

        if (PatientChronicDisease == null)
        {
            return Result.Fail(MessageCodes.Business.CHRONIC_DISEASE_NOT_FOUND);
        }

        _context.PatientChronicDiseases.Remove(PatientChronicDisease);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}