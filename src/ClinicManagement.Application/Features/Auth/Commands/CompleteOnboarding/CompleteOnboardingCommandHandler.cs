using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.CompleteOnboarding;

public class CompleteOnboardingCommandHandler : IRequestHandler<CompleteOnboardingCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILocationsService _locationsService;
    private readonly IClinicManagementService _clinicManagementService;
    private readonly ILogger<CompleteOnboardingCommandHandler> _logger;

    public CompleteOnboardingCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILocationsService locationsService,
        IClinicManagementService clinicManagementService,
        ILogger<CompleteOnboardingCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _locationsService = locationsService;
        _clinicManagementService = clinicManagementService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CompleteOnboardingCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.TryGetUserId(out var userId))
        {
            _logger.LogWarning("Unauthenticated user attempted to complete onboarding");
            return Result<Guid>.Fail(MessageCodes.Authentication.USER_NOT_AUTHENTICATED);
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found during onboarding", userId);
            return Result<Guid>.Fail(MessageCodes.Authentication.USER_NOT_FOUND);
        }

        if (user.ClinicId != null)
        {
            _logger.LogWarning("User {UserId} already has clinic {ClinicId}", userId, user.ClinicId);
            return Result<Guid>.Fail(MessageCodes.Onboarding.USER_ALREADY_HAS_CLINIC);
        }

        var subscriptionPlan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(request.SubscriptionPlanId, cancellationToken);
        if (subscriptionPlan == null || !subscriptionPlan.IsActive)
        {
            _logger.LogWarning("Invalid subscription plan {PlanId} for user {UserId}", request.SubscriptionPlanId, userId);
            return Result<Guid>.Fail(MessageCodes.Onboarding.INVALID_SUBSCRIPTION_PLAN);
        }

        var countries = await _locationsService.GetCountriesAsync();
        var country = countries.FirstOrDefault(c => c.Id == request.CountryId);
        
        var cities = await _locationsService.GetCitiesAsync(request.CountryId, request.StateId);
        var city = cities.FirstOrDefault(c => c.Id == request.CityId);

        var clinic = new Clinic
        {
            Name = request.ClinicName,
            SubscriptionPlanId = request.SubscriptionPlanId,
            IsActive = true
        };

        await _unitOfWork.Clinics.AddAsync(clinic, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var branch = new ClinicBranch
        {
            ClinicId = clinic.Id,
            Name = request.BranchName,
            GeoNameId = request.CityId,
            CityName = city?.Name ?? "Unknown",
            CountryCode = country?.Code ?? "Unknown",
            Address = request.BranchAddress,
            Latitude = city?.Latitude ?? 0,
            Longitude = city?.Longitude ?? 0
        };

        await _unitOfWork.ClinicBranches.AddAsync(branch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (request.BranchPhoneNumbers != null)
        {
            foreach (var phoneDto in request.BranchPhoneNumbers)
            {
                var phone = new ClinicBranchPhoneNumber
                {
                    ClinicBranchId = branch.Id,
                    PhoneNumber = phoneDto.PhoneNumber,
                    Label = phoneDto.Label ?? "Main"
                };
                await _unitOfWork.ClinicBranchPhoneNumbers.AddAsync(phone, cancellationToken);
            }
        }
        
        user.ClinicId = clinic.Id;
        user.CurrentClinicId = clinic.Id; // Set as current clinic
        user.Country = country?.Name;
        user.City = city?.Name;
        
        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);

        // Create UserClinic relationship with owner privileges
        var assignResult = await _clinicManagementService.AssignUserToClinicAsync(userId, clinic.Id, isOwner: true, cancellationToken);
        if (!assignResult.Success)
        {
            _logger.LogError("Failed to assign user {UserId} as owner of clinic {ClinicId}", userId, clinic.Id);
            return Result<Guid>.Fail(assignResult.Code ?? MessageCodes.Business.OPERATION_NOT_ALLOWED);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Onboarding completed for user {UserId}, created clinic {ClinicId} with owner privileges", userId, clinic.Id);

        return Result<Guid>.Ok(clinic.Id);
    }
}
