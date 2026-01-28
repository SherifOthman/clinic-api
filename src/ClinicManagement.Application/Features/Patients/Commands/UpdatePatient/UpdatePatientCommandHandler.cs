using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Patients.Commands.UpdatePatient;

public class UpdatePatientCommandHandler : IRequestHandler<UpdatePatientCommand, Result<PatientDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPhoneNumberValidationService _phoneNumberValidationService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<UpdatePatientCommandHandler> _logger;

    public UpdatePatientCommandHandler(
        IUnitOfWork unitOfWork, 
        ICurrentUserService currentUserService,
        IPhoneNumberValidationService phoneNumberValidationService,
        IDateTimeProvider dateTimeProvider,
        ILogger<UpdatePatientCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _phoneNumberValidationService = phoneNumberValidationService;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<Result<PatientDto>> Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.TryGetUserId(out var userId))
        {
            _logger.LogWarning("Unauthenticated user attempted to update patient {PatientId}", request.Id);
            return Result<PatientDto>.Fail(MessageCodes.Authentication.USER_NOT_AUTHENTICATED);
        }

        if (!_currentUserService.HasClinicAccess())
        {
            _logger.LogWarning("User {UserId} without clinic access attempted to update patient {PatientId}", userId, request.Id);
            return Result<PatientDto>.Fail(MessageCodes.Authorization.USER_NO_CLINIC_ACCESS);
        }

        if (!_currentUserService.TryGetClinicId(out var clinicId))
        {
            _logger.LogWarning("User {UserId} without clinic ID attempted to update patient {PatientId}", userId, request.Id);
            return Result<PatientDto>.Fail(MessageCodes.Authorization.USER_CLINIC_NOT_FOUND);
        }

        var patient = await _unitOfWork.Patients.GetByIdAsync(request.Id, cancellationToken);
        if (patient == null || patient.ClinicId != clinicId)
        {
            _logger.LogWarning("Patient {PatientId} not found or access denied for user {UserId} clinic {ClinicId}", request.Id, userId, clinicId);
            return Result<PatientDto>.Fail(MessageCodes.Business.PATIENT_NOT_FOUND);
        }

        // Update basic patient information
        patient.FullName = request.FullName;
        patient.DateOfBirth = request.DateOfBirth;
        patient.Gender = request.Gender;
        patient.Address = request.Address;
        patient.GeoNameId = request.GeoNameId;

        // Update phone numbers
        await UpdatePhoneNumbers(patient, request.PhoneNumbers, cancellationToken);

        // Update chronic diseases
        await UpdateChronicDiseases(patient, request.ChronicDiseaseIds, cancellationToken);

        await _unitOfWork.Patients.UpdateAsync(patient, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Patient {PatientId} updated by user {UserId}", request.Id, userId);

        var updatedPatient = await _unitOfWork.Patients.GetByIdAsync(request.Id, cancellationToken);
        var patientDto = updatedPatient.Adapt<PatientDto>();
        return Result<PatientDto>.Ok(patientDto);
    }

    private async Task UpdatePhoneNumbers(Patient patient, List<UpdatePatientPhoneNumberDto> phoneNumberDtos, CancellationToken cancellationToken)
    {
        // Remove existing phone numbers that are not in the update list
        var existingPhoneNumbers = patient.PhoneNumbers.ToList();
        var phoneNumbersToRemove = existingPhoneNumbers
            .Where(existing => !phoneNumberDtos.Any(dto => dto.Id == existing.Id))
            .ToList();

        foreach (var phoneNumberToRemove in phoneNumbersToRemove)
        {
            patient.PhoneNumbers.Remove(phoneNumberToRemove);
        }

        // Update existing and add new phone numbers
        foreach (var phoneNumberDto in phoneNumberDtos)
        {
            var formattedPhoneNumber = _phoneNumberValidationService.GetE164Format(phoneNumberDto.PhoneNumber);

            if (phoneNumberDto.Id.HasValue)
            {
                // Update existing phone number
                var existingPhoneNumber = patient.PhoneNumbers.FirstOrDefault(p => p.Id == phoneNumberDto.Id.Value);
                if (existingPhoneNumber != null)
                {
                    existingPhoneNumber.PhoneNumber = formattedPhoneNumber;
                }
            }
            else
            {
                // Add new phone number
                patient.PhoneNumbers.Add(new PatientPhoneNumber
                {
                    PhoneNumber = formattedPhoneNumber,
                    PatientId = patient.Id
                });
            }
        }
    }

    private async Task UpdateChronicDiseases(Patient patient, List<int> chronicDiseaseIds, CancellationToken cancellationToken)
    {
        // Remove all existing chronic diseases
        patient.ChronicDiseases.Clear();

        // Add new chronic diseases
        if (chronicDiseaseIds?.Any() == true)
        {
            foreach (var chronicDiseaseId in chronicDiseaseIds)
            {
                patient.ChronicDiseases.Add(new PatientChronicDisease
                {
                    PatientId = patient.Id,
                    ChronicDiseaseId = chronicDiseaseId,
                    DiagnosedDate = _dateTimeProvider.UtcNow,
                    IsActive = true
                });
            }
        }
    }
}
