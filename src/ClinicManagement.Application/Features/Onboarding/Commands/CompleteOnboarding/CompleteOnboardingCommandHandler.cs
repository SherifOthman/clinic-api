using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Onboarding.Commands.CompleteOnboarding;

public class CompleteOnboardingCommandHandler : IRequestHandler<CompleteOnboardingCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILocationReferenceService _locationReferenceService;

    public CompleteOnboardingCommandHandler(
        IUnitOfWork unitOfWork, 
        ICurrentUserService currentUserService,
        ILocationReferenceService locationReferenceService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _locationReferenceService = locationReferenceService;
    }

    public async Task<Result> Handle(CompleteOnboardingCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        
        // Get current user
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return Result.Fail(MessageCodes.Authentication.USER_NOT_AUTHENTICATED);
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
        if (user == null)
        {
            return Result.Fail(MessageCodes.Authentication.USER_NOT_FOUND);
        }

        // Verify user is a clinic owner
        if (user.UserType != UserType.ClinicOwner)
        {
            return Result.Fail(MessageCodes.Authorization.ACCESS_DENIED);
        }

        // Check if already onboarded
        if (user.OnboardingCompleted)
        {
            return Result.Fail(MessageCodes.Business.OPERATION_NOT_ALLOWED);
        }

        // Validate subscription plan
        if (!Guid.TryParse(dto.SubscriptionPlanId, out var subscriptionPlanId))
        {
            return Result.FailField(nameof(dto.SubscriptionPlanId), MessageCodes.Validation.INVALID_FORMAT);
        }

        var subscriptionPlan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(subscriptionPlanId);
        if (subscriptionPlan == null || !subscriptionPlan.IsActive)
        {
            return Result.FailField(nameof(dto.SubscriptionPlanId), MessageCodes.Business.ENTITY_NOT_FOUND);
        }

        // Resolve location references at runtime (GeoNames snapshot architecture)
        // This creates Country -> State -> City hierarchy ONLY if they don't exist
        // Triggered by user action (onboarding), NOT at startup or migration time
        var cityId = await _locationReferenceService.ResolveLocationAsync(
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

        // Update user's ClinicId
        user.ClinicId = clinic.Id;
        user.OnboardingCompleted = true;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create clinic branch with CityId only (Country/State derived through joins)
        var branch = new ClinicBranch
        {
            ClinicId = clinic.Id,
            Name = dto.BranchName,
            Address = dto.BranchAddress,
            CityId = cityId // Store only CityId - snapshot architecture
        };

        await _unitOfWork.ClinicBranches.AddAsync(branch);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Add phone numbers
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

        // Create ClinicOwner record
        var clinicOwner = new ClinicOwner
        {
            UserId = user.Id
        };
        await _unitOfWork.ClinicOwners.AddAsync(clinicOwner);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
