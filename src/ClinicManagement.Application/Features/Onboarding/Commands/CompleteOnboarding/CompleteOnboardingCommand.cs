using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Onboarding.Commands.CompleteOnboarding;

public record CompleteOnboardingCommand(CompleteOnboardingDto Dto) : IRequest<Result>;


public class CompleteOnboardingCommandHandler : IRequestHandler<CompleteOnboardingCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CompleteOnboardingCommandHandler> _logger;

    public CompleteOnboardingCommandHandler(
        IApplicationDbContext context, 
        ICurrentUserService currentUserService,
        ILogger<CompleteOnboardingCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
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

        var user = await _context.Users.FindAsync(new object[] { userId.Value }, cancellationToken);
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

        var subscriptionPlan = await _context.SubscriptionPlans.FindAsync(new object[] { subscriptionPlanId }, cancellationToken);
        if (subscriptionPlan == null || !subscriptionPlan.IsActive)
        {
            _logger.LogWarning("Onboarding failed: Subscription plan not found or inactive - PlanId: {PlanId}", 
                subscriptionPlanId);
            return Result.FailField(nameof(dto.SubscriptionPlanId), MessageCodes.Business.ENTITY_NOT_FOUND);
        }

        _logger.LogInformation("Subscription plan validated: {PlanName} (ID: {PlanId})", 
            subscriptionPlan.Name, subscriptionPlanId);

        // Create clinic (ID generated in constructor)
        var clinic = new Clinic
        {
            Name = dto.ClinicName,
            OwnerUserId = user.Id,
            SubscriptionPlanId = subscriptionPlanId
        };
        _context.Clinics.Add(clinic);

        // Update user's ClinicId and onboarding status
        user.ClinicId = clinic.Id;
        user.OnboardingCompleted = true;

        // Create clinic branch (ID generated in constructor)
        var branch = new ClinicBranch
        {
            ClinicId = clinic.Id,
            Name = dto.BranchName,
            AddressLine = dto.BranchAddress,
            CountryGeoNameId = dto.Location.CountryGeonameId,
            StateGeoNameId = dto.Location.StateGeonameId,
            CityGeoNameId = dto.Location.CityGeonameId
        };
        _context.ClinicBranches.Add(branch);

        // Add branch phone numbers
        foreach (var phoneDto in dto.BranchPhoneNumbers)
        {
            var phone = new ClinicBranchPhoneNumber
            {
                ClinicBranchId = branch.Id,
                PhoneNumber = phoneDto.PhoneNumber,
                Label = phoneDto.Label
            };
            _context.ClinicBranchPhoneNumbers.Add(phone);
        }

        // Create ClinicOwner record
        var clinicOwner = new ClinicOwner
        {
            UserId = user.Id
        };
        _context.ClinicOwners.Add(clinicOwner);

        // Single SaveChanges - atomic transaction for all operations
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Onboarding completed successfully - Clinic: {ClinicId}, Branch: {BranchId}, Phones: {PhoneCount}",
            clinic.Id, branch.Id, dto.BranchPhoneNumbers.Count);

        return Result.Ok();
    }
}