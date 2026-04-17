using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Application.Features.Onboarding.Commands;

public class CompleteOnboardingHandler : IRequestHandler<CompleteOnboarding, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<User> _userManager;

    public CompleteOnboardingHandler(IUnitOfWork uow, ICurrentUserService currentUserService, UserManager<User> userManager)
    {
        _uow                = uow;
        _currentUserService = currentUserService;
        _userManager        = userManager;
    }

    public async Task<Result> Handle(CompleteOnboarding request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();

        if (await _uow.Clinics.ExistsByOwnerIdAsync(userId, cancellationToken))
            return Result.Failure(ErrorCodes.ALREADY_ONBOARDED, "User has already completed onboarding");

        var user = await _uow.Users.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result.Failure(ErrorCodes.USER_NOT_FOUND, "User not found");

        var userRoles = await _userManager.GetRolesAsync(user);
        if (!userRoles.Contains(UserRoles.ClinicOwner))
            return Result.Failure(ErrorCodes.FORBIDDEN, "User must be clinic owner");

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
        });

        var ownerMember = CreateOwnerMember(user, clinic.Id);
        await _uow.Members.AddAsync(ownerMember);

        if (request.ProvideMedicalServices)
        {
            await _uow.DoctorInfos.AddAsync(new DoctorInfo
            {
                ClinicMemberId   = ownerMember.Id,
                SpecializationId = request.SpecializationId,
            });
        }

        await _uow.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private static ClinicMember CreateOwnerMember(User user, Guid clinicId)
    {
        return new ClinicMember
        {
            PersonId = user.PersonId,
            UserId   = user.Id,
            ClinicId = clinicId,
            Role     = Domain.Enums.ClinicMemberRole.Owner,
            IsActive = true,
        };
    }
}
