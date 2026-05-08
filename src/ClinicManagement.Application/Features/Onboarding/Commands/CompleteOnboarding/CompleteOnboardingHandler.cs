using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Onboarding.Commands;

public class CompleteOnboardingHandler : IRequestHandler<CompleteOnboarding, Result>
{
    private readonly IClinicRepository       _clinics;
    private readonly IBranchRepository       _branches;
    private readonly IClinicMemberRepository _members;
    private readonly IPermissionRepository   _permissions;
    private readonly IUserRepository         _users;
    private readonly IReferenceRepository    _reference;
    private readonly IUnitOfWork             _uow;
    private readonly ICurrentUserService     _currentUserService;

    public CompleteOnboardingHandler(
        IClinicRepository clinics,
        IBranchRepository branches,
        IClinicMemberRepository members,
        IPermissionRepository permissions,
        IUserRepository users,
        IReferenceRepository reference,
        IUnitOfWork uow,
        ICurrentUserService currentUserService)
    {
        _clinics            = clinics;
        _branches           = branches;
        _members            = members;
        _permissions        = permissions;
        _users              = users;
        _reference          = reference;
        _uow                = uow;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(CompleteOnboarding request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();

        if (await _clinics.ExistsByOwnerIdAsync(userId, cancellationToken))
            return Result.Failure(ErrorCodes.ALREADY_ONBOARDED, "User has already completed onboarding");

        var user = await _users.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result.Failure(ErrorCodes.USER_NOT_FOUND, "User not found");

        if (!await _reference.SubscriptionPlanExistsAsync(request.SubscriptionPlanId, cancellationToken))
            return Result.Failure(ErrorCodes.PLAN_NOT_FOUND, "The selected subscription plan does not exist");

        // Clinic.Create() owns construction — consistent with StaffInvitation.Create() pattern
        var clinic = Clinic.Create(request.ClinicName, userId, request.SubscriptionPlanId, request.CountryCode);
        await _clinics.AddAsync(clinic);

        var phoneNumbers = request.PhoneNumbers?
            .Where(p => !string.IsNullOrWhiteSpace(p.PhoneNumber))
            .Select(p => new ClinicBranchPhoneNumber { PhoneNumber = p.PhoneNumber.Trim(), Label = p.Label?.Trim() })
            .ToList();

        // ClinicBranch.CreateMain() owns construction
        var branch = ClinicBranch.CreateMain(
            clinic.Id, request.BranchName, request.AddressLine,
            request.StateGeonameId, request.CityGeonameId, phoneNumbers);
        await _branches.AddAsync(branch);

        // ClinicMember.CreateForOwner() owns construction
        var ownerMember = ClinicMember.CreateForOwner(userId, clinic.Id);
        await _members.AddAsync(ownerMember);

        await _permissions.SeedDefaultsAsync(ownerMember.Id, Domain.Enums.ClinicMemberRole.Owner, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
