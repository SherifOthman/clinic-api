using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Staff.Dtos;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class InviteStaffHandler : IRequestHandler<InviteStaffCommand, Result<InviteStaffResponseDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;
    private readonly ISecurityAuditWriter _auditWriter;

    public InviteStaffHandler(IUnitOfWork uow, ICurrentUserService currentUserService, IEmailService emailService, ISecurityAuditWriter auditWriter)
    {
        _uow                = uow;
        _currentUserService = currentUserService;
        _emailService       = emailService;
        _auditWriter        = auditWriter;
    }

    public async Task<Result<InviteStaffResponseDto>> Handle(InviteStaffCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.GetRequiredUserId();
        var clinicId      = _currentUserService.GetRequiredClinicId();

        var role = request.Role switch
        {
            UserRoles.Doctor       => ClinicMemberRole.Doctor,
            UserRoles.Receptionist => ClinicMemberRole.Receptionist,
            _                      => ClinicMemberRole.Receptionist,
        };

        var invitation = StaffInvitation.Create(clinicId, request.Email, role, currentUserId, request.SpecializationId);
        await _uow.Invitations.AddAsync(invitation);

        var inviter = await _uow.Users.GetByIdWithPersonAsync(currentUserId, cancellationToken);
        var clinic  = await _uow.Clinics.GetByIdAsync(clinicId, cancellationToken);

        await _emailService.SendStaffInvitationEmailAsync(
            request.Email,
            clinic?.Name ?? "Clinic",
            request.Role,
            inviter?.Person.FullName ?? "Clinic Administrator",
            $"/accept-invitation/{invitation.InvitationToken}",
            cancellationToken);

        await _uow.SaveChangesAsync(cancellationToken);

        await _auditWriter.WriteAsync(
            currentUserId, _currentUserService.FullName, _currentUserService.Username, _currentUserService.Email,
            _currentUserService.Roles.FirstOrDefault(), clinicId,
            "StaffInvited", $"Invited {request.Email} as {request.Role}", cancellationToken);

        return Result.Success(new InviteStaffResponseDto(invitation.Id, invitation.InvitationToken, invitation.ExpiresAt));
    }
}
