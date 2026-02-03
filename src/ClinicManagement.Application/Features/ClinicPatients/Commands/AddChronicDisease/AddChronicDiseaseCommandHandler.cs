using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.ClinicPatients.Commands.AddChronicDisease;

public class AddChronicDiseaseCommandHandler : IRequestHandler<AddChronicDiseaseCommand, Result<ClinicPatientChronicDiseaseDto>>
{
    private readonly IClinicPatientChronicDiseaseRepository _clinicPatientChronicDiseaseRepository;
    private readonly IChronicDiseaseRepository _chronicDiseaseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddChronicDiseaseCommandHandler(
        IClinicPatientChronicDiseaseRepository clinicPatientChronicDiseaseRepository,
        IChronicDiseaseRepository chronicDiseaseRepository,
        IUnitOfWork unitOfWork)
    {
        _clinicPatientChronicDiseaseRepository = clinicPatientChronicDiseaseRepository;
        _chronicDiseaseRepository = chronicDiseaseRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ClinicPatientChronicDiseaseDto>> Handle(AddChronicDiseaseCommand request, CancellationToken cancellationToken)
    {
        // Check if chronic disease exists
        var chronicDisease = await _chronicDiseaseRepository.GetByIdAsync(request.ChronicDisease.ChronicDiseaseId, cancellationToken);
        if (chronicDisease == null)
        {
            return Result<ClinicPatientChronicDiseaseDto>.FailField("chronicDisease.chronicDiseaseId", MessageCodes.Business.CHRONIC_DISEASE_NOT_FOUND);
        }

        // Check if the relationship already exists
        var exists = await _clinicPatientChronicDiseaseRepository.ExistsAsync(
            request.ClinicPatientId, 
            request.ChronicDisease.ChronicDiseaseId, 
            cancellationToken);

        if (exists)
        {
            return Result<ClinicPatientChronicDiseaseDto>.FailField("chronicDisease.chronicDiseaseId", MessageCodes.Business.CHRONIC_DISEASE_ALREADY_EXISTS);
        }

        // Create the relationship
        var clinicPatientChronicDisease = new ClinicPatientChronicDisease
        {
            ClinicPatientId = request.ClinicPatientId,
            ChronicDiseaseId = request.ChronicDisease.ChronicDiseaseId,
            DiagnosedDate = request.ChronicDisease.DiagnosedDate,
            Status = request.ChronicDisease.Status,
            Notes = request.ChronicDisease.Notes,
            IsActive = request.ChronicDisease.IsActive
        };

        await _clinicPatientChronicDiseaseRepository.AddAsync(clinicPatientChronicDisease, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Get the created entity with navigation properties
        var createdEntity = await _clinicPatientChronicDiseaseRepository.GetByClinicPatientAndDiseaseAsync(
            request.ClinicPatientId, 
            request.ChronicDisease.ChronicDiseaseId, 
            cancellationToken);

        var dto = createdEntity!.Adapt<ClinicPatientChronicDiseaseDto>();
        return Result<ClinicPatientChronicDiseaseDto>.Ok(dto);
    }
}
