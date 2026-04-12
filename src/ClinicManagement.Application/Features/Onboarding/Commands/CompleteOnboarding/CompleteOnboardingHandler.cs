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
            Name               = request.ClinicName,
            OwnerUserId        = userId,
            SubscriptionPlanId = request.SubscriptionPlanId,
            OnboardingCompleted = true,
            IsActive           = true,
        };
        await _uow.Clinics.AddAsync(clinic);

        var branch = new ClinicBranch
        {
            ClinicId     = clinic.Id,
            Name         = request.BranchName,
            AddressLine  = request.AddressLine,
            CityNameEn   = request.CityNameEn,
            CityNameAr   = request.CityNameAr,
            StateNameEn  = request.StateNameEn,
            StateNameAr  = request.StateNameAr,
            IsMainBranch = true,
            IsActive     = true,
        };
        await _uow.Branches.AddAsync(branch);

        if (request.ProvideMedicalServices)
        {
            var staff = new Domain.Entities.Staff { UserId = userId, ClinicId = clinic.Id, IsActive = true };
            await _uow.Staff.AddAsync(staff);

            await _uow.DoctorProfiles.AddAsync(new DoctorProfile
            {
                StaffId          = staff.Id,
                SpecializationId = request.SpecializationId,
                CreatedAt        = DateTimeOffset.UtcNow,
            });
        }

        await _uow.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
