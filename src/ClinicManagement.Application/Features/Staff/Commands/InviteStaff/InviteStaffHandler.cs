using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Staff.Dtos;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class InviteStaffHandler : IRequestHandler<InviteStaffCommand, Result<InviteStaffResponseDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;

    public InviteStaffHandler(IUnitOfWork uow, ICurrentUserService currentUserService, IEmailService emailService)
    {
        _uow                = uow;
        _currentUserService = currentUserService;
        _emailService       = emailService;
    }

    public async Task<Result<InviteStaffResponseDto>> Handle(InviteStaffCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.GetRequiredUserId();
        var clinicId      = _currentUserService.GetRequiredClinicId();

        var invitation = StaffInvitation.Create(clinicId, request.Email, request.Role, currentUserId, request.SpecializationId);
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

        return Result.Success(new InviteStaffResponseDto(invitation.Id, invitation.InvitationToken, invitation.ExpiresAt));
    }
}
