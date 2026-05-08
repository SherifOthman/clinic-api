using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class ResendInvitationHandler : IRequestHandler<ResendInvitationCommand, Result>
{
    private readonly IInvitationRepository _invitations;
    private readonly IUserRepository       _users;
    private readonly IClinicRepository     _clinics;
    private readonly IUnitOfWork           _uow;
    private readonly ICurrentUserService   _currentUserService;
    private readonly IEmailService         _emailService;

    public ResendInvitationHandler(
        IInvitationRepository invitations,
        IUserRepository users,
        IClinicRepository clinics,
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        IEmailService emailService)
    {
        _invitations        = invitations;
        _users              = users;
        _clinics            = clinics;
        _uow                = uow;
        _currentUserService = currentUserService;
        _emailService       = emailService;
    }

    public async Task<Result> Handle(ResendInvitationCommand request, CancellationToken cancellationToken)
    {
        var clinicId      = _currentUserService.GetRequiredClinicId();
        var currentUserId = _currentUserService.GetRequiredUserId();

        var invitation = await _invitations.GetByIdWithSpecializationAsync(request.InvitationId, cancellationToken);

        if (invitation is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Invitation not found");

        if (invitation.ClinicId != clinicId)
            return Result.Failure(ErrorCodes.FORBIDDEN, "You can only resend invitations from your own clinic");

        if (invitation.IsAccepted || invitation.IsCanceled)
            return Result.Failure(ErrorCodes.OPERATION_NOT_ALLOWED, "Cannot resend an accepted or cancelled invitation");

        invitation.ExpiresAt = DateTimeOffset.UtcNow.AddDays(7);

        var inviter = await _users.GetByIdAsync(currentUserId, cancellationToken);
        var clinic  = await _clinics.GetByIdAsync(clinicId, cancellationToken);

        // Save the extended expiry first
        await _uow.SaveChangesAsync(cancellationToken);

        await _emailService.SendStaffInvitationEmailAsync(
            invitation.Email,
            clinic?.Name ?? "Clinic",
            invitation.Role.ToString(),
            inviter?.FullName ?? "Clinic Administrator",
            $"/en/accept-invitation/{invitation.InvitationToken}",
            cancellationToken);

        return Result.Success();
    }
}
