using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Onboarding.Commands;

public class CompleteOnboardingHandler : IRequestHandler<CompleteOnboarding, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;

    public CompleteOnboardingHandler(IUnitOfWork uow, ICurrentUserService currentUserService)
    {
        _uow                = uow;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(CompleteOnboarding request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();

        if (await _uow.Clinics.ExistsByOwnerIdAsync(userId, cancellationToken))
            return Result.Failure(ErrorCodes.ALREADY_ONBOARDED, "User has already completed onboarding");

        var user = await _uow.Users.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result.Failure(ErrorCodes.USER_NOT_FOUND, "User not found");

        if (!await _uow.Reference.SubscriptionPlanExistsAsync(request.SubscriptionPlanId, cancellationToken))
            return Result.Failure(ErrorCodes.PLAN_NOT_FOUND, "The selected subscription plan does not exist");

        var clinic = new Clinic
        {
            Name                = request.ClinicName,
            OwnerUserId         = userId,
            SubscriptionPlanId  = request.SubscriptionPlanId,
            OnboardingCompleted = true,
            IsActive            = true,
            CountryCode         = request.CountryCode,
        };
        await _uow.Clinics.AddAsync(clinic);

        await _uow.Branches.AddAsync(new ClinicBranch
        {
            ClinicId       = clinic.Id,
            Name           = request.BranchName,
            AddressLine    = request.AddressLine,
            StateGeonameId = request.StateGeonameId,
            CityGeonameId  = request.CityGeonameId,
            IsMainBranch   = true,
            IsActive       = true,
            PhoneNumbers   = request.PhoneNumbers?
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => new ClinicBranchPhoneNumber { PhoneNumber = p.Trim() })
                .ToList() ?? [],
        });

        var ownerMember = CreateOwnerMember(user, clinic.Id);
        await _uow.Members.AddAsync(ownerMember);

        await _uow.Permissions.SeedDefaultsAsync(ownerMember.Id, Domain.Enums.ClinicMemberRole.Owner, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        // Audit captured automatically by SaveChanges interceptor (Clinic + Branch creation)
        return Result.Success();
    }

    private static ClinicMember CreateOwnerMember(User user, Guid clinicId) => new()
    {
        UserId   = user.Id,
        ClinicId = clinicId,
        Role     = Domain.Enums.ClinicMemberRole.Owner,
        IsActive = true,
    };
}
