using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using MediatR;

namespace ClinicManagement.Application.Onboarding.Commands;

public record CompleteOnboarding(
    string ClinicName,
    Guid SubscriptionPlanId,
    string BranchName,
    string AddressLine,
    int CountryGeoNameId,
    int StateGeoNameId,
    int CityGeoNameId
) : IRequest<Result>;

public class CompleteOnboardingHandler : IRequestHandler<CompleteOnboarding, Result>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteOnboardingHandler(
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CompleteOnboarding request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();

        var existingClinic = await _unitOfWork.Clinics.GetByOwnerUserIdAsync(userId, cancellationToken);
        if (existingClinic != null)
        {
            return Result.Failure(ErrorCodes.ALREADY_ONBOARDED, "User has already completed onboarding");
        }

       var userRoles =  await _unitOfWork.Users.GetUserRolesAsync(userId);

        if (!userRoles.Contains(UserRoles.ClinicOwner))
        {
            return Result.Failure(ErrorCodes.FORBIDDEN, "User must be clinic owner");
        }

        var subscriptionPlan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(request.SubscriptionPlanId, cancellationToken);
        if (subscriptionPlan == null)
        {
            return Result.Failure(ErrorCodes.PLAN_NOT_FOUND, "The selected subscription plan does not exist");
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var clinic = new Clinic
            {
                Name = request.ClinicName,
                OwnerUserId = userId,
                SubscriptionPlanId = request.SubscriptionPlanId,
                OnboardingCompleted = true,
                IsActive = true
            };

            await _unitOfWork.Clinics.AddAsync(clinic, cancellationToken);

            var branch = new ClinicBranch
            {
                ClinicId = clinic.Id,
                Name = request.BranchName,
                AddressLine = request.AddressLine,
                CountryGeoNameId = request.CountryGeoNameId,
                StateGeoNameId = request.StateGeoNameId,
                CityGeoNameId = request.CityGeoNameId,
                IsMainBranch = true,
                IsActive = true
            };

            await _unitOfWork.ClinicBranches.AddAsync(branch, cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
