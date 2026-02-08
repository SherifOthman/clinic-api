using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Onboarding.Commands.CompleteOnboarding;

public class CompleteOnboardingCommandHandler : IRequestHandler<CompleteOnboardingCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILocationReferenceService _locationReferenceService;
    private readonly ILogger<CompleteOnboardingCommandHandler> _logger;

    public CompleteOnboardingCommandHandler(
        IUnitOfWork unitOfWork, 
        ICurrentUserService currentUserService,
        ILocationReferenceService locationReferenceService,
        ILogger<CompleteOnboardingCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _locationReferenceService = locationReferenceService;
        _logger = logger;
    }

    public async Task<Result> Handle(CompleteOnboardingCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        
        _logger.LogInformation("Starting onboarding process for clinic: {ClinicName}", dto.ClinicName);
        
        // Get current user
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            _logger.LogWarning("Onboarding failed: User not authenticated");
            return Result.Fail(MessageCodes.Authentication.USER_NOT_AUTHENTICATED);
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
        if (user == null)
        {
            _logger.LogWarning("Onboarding failed: User not found - UserId: {UserId}", userId.Value);
            return Result.Fail(MessageCodes.Authentication.USER_NOT_FOUND);
        }

        _logger.LogInformation("Processing onboarding for user: {UserId}, Email: {Email}", user.Id, user.Email);

        // Verify user is a clinic owner
        if (user.UserType != UserType.ClinicOwner)
        {
            _logger.LogWarning("Onboarding failed: User is not a clinic owner - UserId: {UserId}, UserType: {UserType}", 
                user.Id, user.UserType);
            return Result.Fail(MessageCodes.Authorization.ACCESS_DENIED);
        }

        // Check if already onboarded
        if (user.OnboardingCompleted)
        {
            _logger.LogWarning("Onboarding failed: User already completed onboarding - UserId: {UserId}", user.Id);
            return Result.Fail(MessageCodes.Business.OPERATION_NOT_ALLOWED);
        }

        // Validate subscription plan
        if (!Guid.TryParse(dto.SubscriptionPlanId, out var subscriptionPlanId))
        {
            _logger.LogWarning("Onboarding failed: Invalid subscription plan ID format - Value: {SubscriptionPlanId}", 
                dto.SubscriptionPlanId);
            return Result.FailField(nameof(dto.SubscriptionPlanId), MessageCodes.Validation.INVALID_FORMAT);
        }

        var subscriptionPlan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(subscriptionPlanId);
        if (subscriptionPlan == null || !subscriptionPlan.IsActive)
        {
            _logger.LogWarning("Onboarding failed: Subscription plan not found or inactive - PlanId: {PlanId}", 
                subscriptionPlanId);
            return Result.FailField(nameof(dto.SubscriptionPlanId), MessageCodes.Business.ENTITY_NOT_FOUND);
        }

        _logger.LogInformation("Subscription plan validated: {PlanName} (ID: {PlanId})", 
            subscriptionPlan.Name, subscriptionPlanId);

        // Create location snapshots for display purposes
        await _locationReferenceService.ResolveLocationAsync(
            dto.Location.CountryGeonameId,
            new CountrySnapshotData(
                dto.Location.CountryIso2Code,
                dto.Location.CountryPhoneCode,
                dto.Location.CountryNameEn,
                dto.Location.CountryNameAr),
            dto.Location.StateGeonameId,
            new StateSnapshotData(
                dto.Location.StateNameEn,
                dto.Location.StateNameAr),
            dto.Location.CityGeonameId,
            new CitySnapshotData(
                dto.Location.CityNameEn,
                dto.Location.CityNameAr),
            cancellationToken);

        // Create clinic
        var clinic = new Clinic
        {
            Name = dto.ClinicName,
            OwnerUserId = user.Id,
            SubscriptionPlanId = subscriptionPlanId
        };

        await _unitOfWork.Clinics.AddAsync(clinic);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Clinic created successfully - ClinicId: {ClinicId}, Name: {ClinicName}", 
            clinic.Id, clinic.Name);

        // Update user's ClinicId
        user.ClinicId = clinic.Id;
        user.OnboardingCompleted = true;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User updated with ClinicId and onboarding completed - UserId: {UserId}", user.Id);

        var branch = new ClinicBranch
        {
            ClinicId = clinic.Id,
            Name = dto.BranchName,
            AddressLine = dto.BranchAddress,
            CountryGeoNameId = dto.Location.CountryGeonameId,
            StateGeoNameId = dto.Location.StateGeonameId,
            CityGeoNameId = dto.Location.CityGeonameId
        };

        await _unitOfWork.ClinicBranches.AddAsync(branch);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Clinic branch created - BranchId: {BranchId}, Name: {BranchName}", 
            branch.Id, branch.Name);

        foreach (var phoneDto in dto.BranchPhoneNumbers)
        {
            var phone = new ClinicBranchPhoneNumber
            {
                ClinicBranchId = branch.Id,
                PhoneNumber = phoneDto.PhoneNumber,
                Label = phoneDto.Label
            };
            await _unitOfWork.ClinicBranchPhoneNumbers.AddAsync(phone);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Added {PhoneCount} phone numbers to branch", dto.BranchPhoneNumbers.Count);

        // Create ClinicOwner record
        var clinicOwner = new ClinicOwner
        {
            UserId = user.Id
        };
        await _unitOfWork.ClinicOwners.AddAsync(clinicOwner);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("ClinicOwner record created - UserId: {UserId}", user.Id);
        _logger.LogInformation("Onboarding completed successfully for clinic: {ClinicName} (ID: {ClinicId})", 
            clinic.Name, clinic.Id);

        return Result.Ok();
    }
}
