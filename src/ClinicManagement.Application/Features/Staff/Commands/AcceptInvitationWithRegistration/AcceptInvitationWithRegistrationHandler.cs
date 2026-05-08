using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class AcceptInvitationWithRegistrationHandler : IRequestHandler<AcceptInvitationWithRegistrationCommand, Result>
{
    private readonly IInvitationRepository   _invitations;
    private readonly IClinicMemberRepository _members;
    private readonly IDoctorInfoRepository   _doctorInfos;
    private readonly IPermissionRepository   _permissions;
    private readonly IUnitOfWork             _uow;
    private readonly UserManager<User>       _userManager;
    private readonly IAuditWriter            _audit;

    public AcceptInvitationWithRegistrationHandler(
        IInvitationRepository invitations,
        IClinicMemberRepository members,
        IDoctorInfoRepository doctorInfos,
        IPermissionRepository permissions,
        IUnitOfWork uow,
        UserManager<User> userManager,
        IAuditWriter audit)
    {
        _invitations = invitations;
        _members     = members;
        _doctorInfos = doctorInfos;
        _permissions = permissions;
        _uow         = uow;
        _userManager = userManager;
        _audit       = audit;
    }

    public async Task<Result> Handle(AcceptInvitationWithRegistrationCommand request, CancellationToken cancellationToken)
    {
        var invitation = await _invitations.GetByTokenAsync(request.Token, cancellationToken);
        if (invitation is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Invitation not found");

        var user = new User
        {
            UserName       = request.UserName,
            Email          = invitation.Email,
            PhoneNumber    = request.PhoneNumber,
            EmailConfirmed = true,
            FullName       = request.FullName,
            Gender         = Enum.TryParse<Gender>(request.Gender, out var g) ? g : Gender.Male,
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
            return Result.Failure(ErrorCodes.OPERATION_FAILED,
                string.Join(", ", createResult.Errors.Select(e => e.Description)));

        var roleResult = await _userManager.AddToRoleAsync(user, invitation.Role.ToString());
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return Result.Failure(ErrorCodes.OPERATION_FAILED,
                string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }

        var acceptResult = invitation.Accept(user.Id, DateTimeOffset.UtcNow);
        if (acceptResult.IsFailure) { await _userManager.DeleteAsync(user); return acceptResult; }

        var member = ClinicMember.CreateFromInvitation(user.Id, invitation);
        await _members.AddAsync(member);

        if (invitation.Role == ClinicMemberRole.Doctor)
        {
            await _doctorInfos.AddAsync(new DoctorInfo
            {
                ClinicMemberId   = member.Id,
                SpecializationId = invitation.SpecializationId,
            });
        }

        await _permissions.SeedDefaultsAsync(member.Id, invitation.Role, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        await _audit.WriteEventAsync("StaffInvitationAccepted", $"Role: {invitation.Role}",
            overrideUserId: user.Id, overrideFullName: user.FullName,
            overrideEmail: user.Email, overrideRole: invitation.Role.ToString(),
            overrideClinicId: invitation.ClinicId, ct: cancellationToken);

        return Result.Success();
    }
}
