using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Interfaces;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Commands.RemoveChronicDisease;

public class RemoveChronicDiseaseCommandHandler : IRequestHandler<RemoveChronicDiseaseCommand, Result>
{
    private readonly IPatientChronicDiseaseRepository _PatientChronicDiseaseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveChronicDiseaseCommandHandler(
        IPatientChronicDiseaseRepository PatientChronicDiseaseRepository,
        IUnitOfWork unitOfWork)
    {
        _PatientChronicDiseaseRepository = PatientChronicDiseaseRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveChronicDiseaseCommand request, CancellationToken cancellationToken)
    {
        var PatientChronicDisease = await _PatientChronicDiseaseRepository.GetByPatientAndDiseaseAsync(
            request.PatientId, 
            request.ChronicDiseaseId, 
            cancellationToken);

        if (PatientChronicDisease == null)
        {
            return Result.Fail(MessageCodes.Business.CHRONIC_DISEASE_NOT_FOUND);
        }

        _PatientChronicDiseaseRepository.Delete(PatientChronicDisease);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
