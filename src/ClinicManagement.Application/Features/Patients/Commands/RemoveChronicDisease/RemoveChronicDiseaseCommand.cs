using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Patients.Commands.RemoveChronicDisease;

public record RemoveChronicDiseaseCommand(
    Guid PatientId,
    Guid ChronicDiseaseId
) : IRequest<Result>;

public class RemoveChronicDiseaseCommandHandler : IRequestHandler<RemoveChronicDiseaseCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public RemoveChronicDiseaseCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveChronicDiseaseCommand request, CancellationToken cancellationToken)
    {
        var PatientChronicDisease = await _unitOfWork.Repository<PatientChronicDisease>()
            .FirstOrDefaultAsync(pcd => pcd.PatientId == request.PatientId && pcd.ChronicDiseaseId == request.ChronicDiseaseId, cancellationToken);

        if (PatientChronicDisease == null)
        {
            return Result.FailSystem("NOT_FOUND", "Chronic disease relationship not found");
        }

        _unitOfWork.Repository<PatientChronicDisease>().Delete(PatientChronicDisease);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}