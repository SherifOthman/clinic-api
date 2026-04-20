using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class SetOwnerAsDoctorHandler : IRequestHandler<SetOwnerAsDoctorCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<User> _userManager;

    public SetOwnerAsDoctorHandler(IUnitOfWork uow, ICurrentUserService currentUserService, UserManager<User> userManager)
    {
        _uow                = uow;
        _currentUserService = currentUserService;
        _userManager        = userManager;
    }

    public async Task<Result> Handle(SetOwnerAsDoctorCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();

        var user = await _uow.Users.GetByIdAsync(userId, cancellationToken);
        if (user is null) return Result.Failure(ErrorCodes.USER_NOT_FOUND, "User not found");

        var userRoles = await _userManager.GetRolesAsync(user);
        if (!userRoles.Contains(UserRoles.ClinicOwner))
            return Result.Failure(ErrorCodes.FORBIDDEN, "Only clinic owners can use this endpoint");

        var clinic = await _uow.Clinics.GetByOwnerIdAsync(userId, cancellationToken);
        if (clinic is null)
            return Result.Failure(ErrorCodes.CLINIC_NOT_FOUND, "Clinic not found. Please complete onboarding first.");

        // Check if already a doctor via new model
        var existingMember = await _uow.Members.GetByUserIdAsync(userId, cancellationToken);
        if (existingMember?.DoctorInfo is not null)
            return Result.Failure(ErrorCodes.ALREADY_EXISTS, "You are already registered as a doctor");

        if (!userRoles.Contains(UserRoles.Doctor))
        {
            var roleResult = await _userManager.AddToRoleAsync(user, UserRoles.Doctor);
            if (!roleResult.Succeeded)
                return Result.Failure(ErrorCodes.OPERATION_FAILED, "Failed to assign doctor role");
        }

        if (existingMember is null)
        {
            existingMember = new ClinicMember
            {
                PersonId = user.PersonId,
                UserId   = userId,
                ClinicId = clinic.Id,
                Role     = Domain.Enums.ClinicMemberRole.Owner,
                IsActive = true,
            };
            await _uow.Members.AddAsync(existingMember);
            await _uow.SaveChangesAsync(cancellationToken);
            await _uow.Permissions.SeedDefaultsAsync(existingMember.Id, Domain.Enums.ClinicMemberRole.Owner, cancellationToken);
        }

        await _uow.DoctorInfos.AddAsync(new DoctorInfo
        {
            ClinicMemberId   = existingMember.Id,
            SpecializationId = request.SpecializationId,
        });

        await _uow.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
