using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class SetOwnerAsDoctorHandler : IRequestHandler<SetOwnerAsDoctorCommand, Result>
{
    private readonly IUserRepository         _users;
    private readonly IClinicRepository       _clinics;
    private readonly IClinicMemberRepository _members;
    private readonly IDoctorInfoRepository   _doctorInfos;
    private readonly IPermissionRepository   _permissions;
    private readonly IUnitOfWork             _uow;
    private readonly ICurrentUserService     _currentUserService;
    private readonly UserManager<User>       _userManager;

    public SetOwnerAsDoctorHandler(
        IUserRepository users,
        IClinicRepository clinics,
        IClinicMemberRepository members,
        IDoctorInfoRepository doctorInfos,
        IPermissionRepository permissions,
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        UserManager<User> userManager)
    {
        _users              = users;
        _clinics            = clinics;
        _members            = members;
        _doctorInfos        = doctorInfos;
        _permissions        = permissions;
        _uow                = uow;
        _currentUserService = currentUserService;
        _userManager        = userManager;
    }

    public async Task<Result> Handle(SetOwnerAsDoctorCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();

        var user = await _users.GetByIdAsync(userId, cancellationToken);
        if (user is null) return Result.Failure(ErrorCodes.USER_NOT_FOUND, "User not found");

        var clinic = await _clinics.GetByOwnerIdAsync(userId, cancellationToken);
        if (clinic is null)
            return Result.Failure(ErrorCodes.CLINIC_NOT_FOUND, "Clinic not found. Please complete onboarding first.");

        var existingMember = await _members.GetByUserIdAsync(userId, cancellationToken);
        if (existingMember?.DoctorInfo is not null)
            return Result.Failure(ErrorCodes.ALREADY_EXISTS, "You are already registered as a doctor");

        var userRoles = await _userManager.GetRolesAsync(user);
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
                UserId   = userId,
                ClinicId = clinic.Id,
                Role     = Domain.Enums.ClinicMemberRole.Owner,
                IsActive = true,
            };
            await _members.AddAsync(existingMember);
            await _uow.SaveChangesAsync(cancellationToken);
            await _permissions.SeedDefaultsAsync(existingMember.Id, Domain.Enums.ClinicMemberRole.Owner, cancellationToken);
        }

        await _doctorInfos.AddAsync(new DoctorInfo
        {
            ClinicMemberId   = existingMember.Id,
            SpecializationId = request.SpecializationId,
        });

        await _uow.SaveChangesAsync(cancellationToken);

        _permissions.InvalidateCache(existingMember.Id);

        return Result.Success();
    }
}
