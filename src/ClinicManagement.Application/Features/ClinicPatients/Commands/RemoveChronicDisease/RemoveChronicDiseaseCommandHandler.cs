using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Interfaces;
using MediatR;

namespace ClinicManagement.Application.Features.ClinicPatients.Commands.RemoveChronicDisease;

public class RemoveChronicDiseaseCommandHandler : IRequestHandler<RemoveChronicDiseaseCommand, Result>
{
    private readonly IClinicPatientChronicDiseaseRepository _clinicPatientChronicDiseaseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveChronicDiseaseCommandHandler(
        IClinicPatientChronicDiseaseRepository clinicPatientChronicDiseaseRepository,
        IUnitOfWork unitOfWork)
    {
        _clinicPatientChronicDiseaseRepository = clinicPatientChronicDiseaseRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveChronicDiseaseCommand request, CancellationToken cancellationToken)
    {
        var clinicPatientChronicDisease = await _clinicPatientChronicDiseaseRepository.GetByClinicPatientAndDiseaseAsync(
            request.ClinicPatientId, 
            request.ChronicDiseaseId, 
            cancellationToken);

        if (clinicPatientChronicDisease == null)
        {
            return Result.Fail(MessageCodes.Business.CHRONIC_DISEASE_NOT_FOUND);
        }

        _clinicPatientChronicDiseaseRepository.Delete(clinicPatientChronicDisease);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
