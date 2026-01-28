using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.CompleteOnboarding;

public class CompleteOnboardingCommandHandler : IRequestHandler<CompleteOnboardingCommand, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILocationsService _locationsService;
    private readonly ILogger<CompleteOnboardingCommandHandler> _logger;

    public CompleteOnboardingCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILocationsService locationsService,
        ILogger<CompleteOnboardingCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _locationsService = locationsService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(CompleteOnboardingCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.TryGetUserId(out var userId))
        {
            _logger.LogWarning("Unauthenticated user attempted to complete onboarding");
            return Result<int>.Fail(ApplicationErrors.Authentication.USER_NOT_AUTHENTICATED);
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found during onboarding", userId);
            return Result<int>.Fail(ApplicationErrors.Authentication.USER_NOT_FOUND);
        }

        if (user.ClinicId != null)
        {
            _logger.LogWarning("User {UserId} already has clinic {ClinicId}", userId, user.ClinicId);
            return Result<int>.Fail(ApplicationErrors.Onboarding.USER_ALREADY_HAS_CLINIC);
        }

        var subscriptionPlan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(request.SubscriptionPlanId, cancellationToken);
        if (subscriptionPlan == null || !subscriptionPlan.IsActive)
        {
            _logger.LogWarning("Invalid subscription plan {PlanId} for user {UserId}", request.SubscriptionPlanId, userId);
            return Result<int>.Fail(ApplicationErrors.Onboarding.INVALID_SUBSCRIPTION_PLAN);
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
        user.Country = country?.Name;
        user.City = city?.Name;
        
        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Onboarding completed for user {UserId}, created clinic {ClinicId}", userId, clinic.Id);

        return Result<int>.Ok(clinic.Id);
    }
}
