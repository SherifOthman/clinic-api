using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.ClinicPatients.Commands.UpdateChronicDisease;

public class UpdateChronicDiseaseCommandHandler : IRequestHandler<UpdateChronicDiseaseCommand, Result<ClinicPatientChronicDiseaseDto>>
{
    private readonly IClinicPatientChronicDiseaseRepository _clinicPatientChronicDiseaseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateChronicDiseaseCommandHandler(
        IClinicPatientChronicDiseaseRepository clinicPatientChronicDiseaseRepository,
        IUnitOfWork unitOfWork)
    {
        _clinicPatientChronicDiseaseRepository = clinicPatientChronicDiseaseRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ClinicPatientChronicDiseaseDto>> Handle(UpdateChronicDiseaseCommand request, CancellationToken cancellationToken)
    {
        var clinicPatientChronicDisease = await _clinicPatientChronicDiseaseRepository.GetByClinicPatientAndDiseaseAsync(
            request.ClinicPatientId, 
            request.ChronicDiseaseId, 
            cancellationToken);

        if (clinicPatientChronicDisease == null)
        {
            return Result<ClinicPatientChronicDiseaseDto>.Fail(MessageCodes.Business.CHRONIC_DISEASE_NOT_FOUND);
        }

        // Update the properties
        clinicPatientChronicDisease.DiagnosedDate = request.UpdateData.DiagnosedDate;
        clinicPatientChronicDisease.Status = request.UpdateData.Status;
        clinicPatientChronicDisease.Notes = request.UpdateData.Notes;
        clinicPatientChronicDisease.IsActive = request.UpdateData.IsActive;

        await _clinicPatientChronicDiseaseRepository.UpdateAsync(clinicPatientChronicDisease, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Get the updated entity with navigation properties
        var updatedEntity = await _clinicPatientChronicDiseaseRepository.GetByClinicPatientAndDiseaseAsync(
            request.ClinicPatientId, 
            request.ChronicDiseaseId, 
            cancellationToken);

        var dto = updatedEntity!.Adapt<ClinicPatientChronicDiseaseDto>();
        return Result<ClinicPatientChronicDiseaseDto>.Ok(dto);
    }
}