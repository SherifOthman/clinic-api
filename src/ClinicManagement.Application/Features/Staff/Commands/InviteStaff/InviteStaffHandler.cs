using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Staff.Dtos;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class InviteStaffHandler : IRequestHandler<InviteStaffCommand, Result<InviteStaffResponseDto>>
{
    private readonly IInvitationRepository _invitations;
    private readonly IUserRepository       _users;
    private readonly IClinicRepository     _clinics;
    private readonly IUnitOfWork           _uow;
    private readonly ICurrentUserService   _currentUser;
    private readonly IEmailService         _emailService;
    private readonly IAuditWriter          _audit;

    public InviteStaffHandler(
        IInvitationRepository invitations,
        IUserRepository users,
        IClinicRepository clinics,
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        IEmailService emailService,
        IAuditWriter audit)
    {
        _invitations  = invitations;
        _users        = users;
        _clinics      = clinics;
        _uow          = uow;
        _currentUser  = currentUser;
        _emailService = emailService;
        _audit        = audit;
    }

    public async Task<Result<InviteStaffResponseDto>> Handle(InviteStaffCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.GetRequiredUserId();
        var clinicId      = _currentUser.GetRequiredClinicId();

        var role = request.Role switch
        {
            UserRoles.Doctor       => ClinicMemberRole.Doctor,
            UserRoles.Receptionist => ClinicMemberRole.Receptionist,
            _                      => ClinicMemberRole.Receptionist,
        };

        var invitation = StaffInvitation.Create(clinicId, request.Email, role, currentUserId, request.SpecializationId);
        await _invitations.AddAsync(invitation);

        var inviter = await _users.GetByIdAsync(currentUserId, cancellationToken);
        var clinic  = await _clinics.GetByIdAsync(clinicId, cancellationToken);

        // Save first — if the DB write fails, no email is sent
        await _uow.SaveChangesAsync(cancellationToken);

        await _emailService.SendStaffInvitationEmailAsync(
            request.Email,
            clinic?.Name ?? "Clinic",
            request.Role,
            inviter?.FullName ?? "Clinic Administrator",
            $"/en/accept-invitation/{invitation.InvitationToken}",
            cancellationToken);

        await _audit.WriteEventAsync("StaffInvited", $"Invited {request.Email} as {request.Role}", ct: cancellationToken);

        return Result.Success(new InviteStaffResponseDto(invitation.Id, invitation.InvitationToken, invitation.ExpiresAt));
    }
}
