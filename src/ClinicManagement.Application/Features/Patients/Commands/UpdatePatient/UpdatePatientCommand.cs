using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Commands.UpdatePatient;

public record UpdatePatientCommand(Guid Id, UpdatePatientDto Dto) : IRequest<Result<PatientDto>>;

public class UpdatePatientCommandHandler : IRequestHandler<UpdatePatientCommand, Result<PatientDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdatePatientCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PatientDto>> Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var clinicId = _currentUserService.ClinicId!.Value;

        // Get patient with related data
        var patient = await _unitOfWork.Patients.GetByIdForClinicAsync(request.Id, clinicId, cancellationToken);

        if (patient == null)
        {
            return Result<PatientDto>.FailSystem("NOT_FOUND", "Patient not found");
        }

        // Load with includes for updating
        patient = await _unitOfWork.Patients.GetByIdWithIncludesAsync(request.Id, cancellationToken);

        // Update basic info using domain method
        patient!.UpdateInfo(dto.FullName, dto.Gender, dto.DateOfBirth, dto.CityGeoNameId);

        // Update phone numbers - remove old ones and add new ones
        var existingPhones = patient.PhoneNumbers.Select(p => p.PhoneNumber).ToList();
        var newPhones = dto.PhoneNumbers.Select(p => p.PhoneNumber).ToList();

        // Remove phones that are no longer in the list
        foreach (var phoneNumber in existingPhones)
        {
            if (!newPhones.Contains(phoneNumber))
            {
                patient.RemovePhoneNumber(phoneNumber);
            }
        }

        // Add new phones that don't exist yet
        foreach (var phoneDto in dto.PhoneNumbers)
        {
            if (!existingPhones.Contains(phoneDto.PhoneNumber))
            {
                patient.AddPhoneNumber(phoneDto.PhoneNumber, phoneDto.IsPrimary);
            }
            else if (phoneDto.IsPrimary)
            {
                // Update primary status if needed
                patient.SetPrimaryPhoneNumber(phoneDto.PhoneNumber);
            }
        }

        // Update chronic diseases - remove old ones and add new ones
        var existingDiseaseIds = patient.ChronicDiseases.Select(cd => cd.ChronicDiseaseId).ToList();
        var newDiseaseIds = dto.ChronicDiseaseIds.ToList();

        // Remove diseases that are no longer in the list
        foreach (var diseaseId in existingDiseaseIds)
        {
            if (!newDiseaseIds.Contains(diseaseId))
            {
                patient.RemoveChronicDisease(diseaseId);
            }
        }

        // Add new diseases that don't exist yet
        foreach (var diseaseId in newDiseaseIds)
        {
            if (!existingDiseaseIds.Contains(diseaseId))
            {
                patient.AddChronicDisease(diseaseId);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with fresh data
        var updatedPatient = await _unitOfWork.Patients.GetByIdWithIncludesAsync(patient.Id, cancellationToken);

        var patientDto = updatedPatient!.Adapt<PatientDto>();
        return Result<PatientDto>.Ok(patientDto);
    }
}
