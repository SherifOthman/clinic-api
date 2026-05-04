using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Staff.Dtos;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class InviteStaffHandler : IRequestHandler<InviteStaffCommand, Result<InviteStaffResponseDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IEmailService _emailService;
    private readonly IAuditWriter _audit;

    public InviteStaffHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        IEmailService emailService,
        IAuditWriter audit)
    {
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
        await _uow.Invitations.AddAsync(invitation);

        var inviter = await _uow.Users.GetByIdAsync(currentUserId, cancellationToken);
        var clinic  = await _uow.Clinics.GetByIdAsync(clinicId, cancellationToken);

        await _emailService.SendStaffInvitationEmailAsync(
            request.Email,
            clinic?.Name ?? "Clinic",
            request.Role,
            inviter?.FullName ?? "Clinic Administrator",
            $"/accept-invitation/{invitation.InvitationToken}",
            cancellationToken);

        await _uow.SaveChangesAsync(cancellationToken);

        // Manual audit — business context ("who was invited as what role") is more useful than the row creation diff
        await _audit.WriteEventAsync("StaffInvited", $"Invited {request.Email} as {request.Role}", ct: cancellationToken);

        return Result.Success(new InviteStaffResponseDto(invitation.Id, invitation.InvitationToken, invitation.ExpiresAt));
    }
}
