using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Common.Interfaces;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Patients.Commands.CreatePatient;

public class CreatePatientCommandHandler : IRequestHandler<CreatePatientCommand, Result<PatientDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPhoneNumberValidationService _phoneNumberValidationService;
    private readonly ILogger<CreatePatientCommandHandler> _logger;

    public CreatePatientCommandHandler(
        IUnitOfWork unitOfWork, 
        ICurrentUserService currentUserService, 
        IDateTimeProvider dateTimeProvider,
        IPhoneNumberValidationService phoneNumberValidationService,
        ILogger<CreatePatientCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _phoneNumberValidationService = phoneNumberValidationService;
        _logger = logger;
    }

    public async Task<Result<PatientDto>> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.TryGetUserId(out var userId))
        {
            _logger.LogWarning("Unauthenticated user attempted to create patient");
            return Result<PatientDto>.Fail("User not authenticated");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

        if (user?.ClinicId == null)
        {
            _logger.LogWarning("User {UserId} without clinic attempted to create patient", userId);
            return Result<PatientDto>.Fail("User must complete onboarding first");
        }

        var patient = request.Adapt<Patient>();
        patient.ClinicId = user.ClinicId.Value;
        
        // Clear any phone numbers that might have been mapped automatically
        patient.PhoneNumbers.Clear();

        // Format phone numbers to E.164 format for consistent storage
        foreach (var phoneNumberDto in request.PhoneNumbers)
        {
            // Check if number fits Egyptian format first, then use EG as default region
            var defaultRegion = _phoneNumberValidationService.IsEgyptianPhoneNumber(phoneNumberDto.PhoneNumber) ? "EG" : null;
            var formattedPhoneNumber = _phoneNumberValidationService.GetE164Format(phoneNumberDto.PhoneNumber, defaultRegion);
            patient.PhoneNumbers.Add(new PatientPhoneNumber
            {
                PhoneNumber = formattedPhoneNumber,
            });
        }

        await _unitOfWork.Patients.AddAsync(patient, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (request.ChronicDiseaseIds?.Any() == true)
        {
            foreach (var chronicDiseaseId in request.ChronicDiseaseIds)
            {
                var patientChronicDisease = new PatientChronicDisease
                {
                    PatientId = patient.Id,
                    ChronicDiseaseId = chronicDiseaseId,
                    IsActive = true,
                    DiagnosedDate = _dateTimeProvider.UtcNow
                };
                patient.ChronicDiseases.Add(patientChronicDisease);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var createdPatient = await _unitOfWork.Patients.GetByIdAsync(patient.Id, cancellationToken);
        var patientDto = createdPatient.Adapt<PatientDto>();

        _logger.LogInformation("Created patient with ID {PatientId} for clinic {ClinicId}", patient.Id, user.ClinicId);

        return Result<PatientDto>.Ok(patientDto);
    }
}
