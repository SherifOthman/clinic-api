using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Commands.CreatePatient;

public record CreatePatientCommand(CreatePatientDto Dto) : IRequest<Result<PatientDto>>;

public class CreatePatientCommandHandler : IRequestHandler<CreatePatientCommand, Result<PatientDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICodeGeneratorService _codeGenerator;

    public CreatePatientCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ICodeGeneratorService codeGenerator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _codeGenerator = codeGenerator;
    }

    public async Task<Result<PatientDto>> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var clinicId = _currentUserService.ClinicId!.Value;

        // Generate unique patient code
        var patientCode = await _codeGenerator.GeneratePatientNumberAsync(clinicId, cancellationToken);

        // Create patient using factory method - this raises PatientRegisteredEvent
        var patient = Patient.Create(
            patientCode,
            clinicId,
            dto.FullName,
            dto.Gender,
            dto.DateOfBirth,
            dto.CityGeoNameId
        );

        // Add phone numbers using behavior method
        foreach (var phoneDto in dto.PhoneNumbers)
        {
            patient.AddPhoneNumber(phoneDto.PhoneNumber, phoneDto.IsPrimary);
        }

        // Add chronic diseases using behavior method
        if (dto.ChronicDiseaseIds.Any())
        {
            foreach (var diseaseId in dto.ChronicDiseaseIds)
            {
                patient.AddChronicDisease(diseaseId);
            }
        }

        await _unitOfWork.Patients.AddAsync(patient, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Load related data for response
        var createdPatient = await _unitOfWork.Patients.GetByIdWithIncludesAsync(patient.Id, cancellationToken);

        var patientDto = createdPatient!.Adapt<PatientDto>();
        return Result<PatientDto>.Ok(patientDto);
    }
}
