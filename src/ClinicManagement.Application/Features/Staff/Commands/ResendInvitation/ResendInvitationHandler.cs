using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class ResendInvitationHandler : IRequestHandler<ResendInvitationCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;

    public ResendInvitationHandler(IUnitOfWork uow, ICurrentUserService currentUserService, IEmailService emailService)
    {
        _uow                = uow;
        _currentUserService = currentUserService;
        _emailService       = emailService;
    }

    public async Task<Result> Handle(ResendInvitationCommand request, CancellationToken cancellationToken)
    {
        var clinicId      = _currentUserService.GetRequiredClinicId();
        var currentUserId = _currentUserService.GetRequiredUserId();

        var invitation = await _uow.Invitations.GetByIdWithSpecializationAsync(request.InvitationId, cancellationToken);

        if (invitation is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Invitation not found");

        if (invitation.ClinicId != clinicId)
            return Result.Failure(ErrorCodes.FORBIDDEN, "You can only resend invitations from your own clinic");

        if (invitation.IsAccepted || invitation.IsCanceled)
            return Result.Failure(ErrorCodes.OPERATION_NOT_ALLOWED, "Cannot resend an accepted or cancelled invitation");

        invitation.ExpiresAt = DateTimeOffset.UtcNow.AddDays(7);

        var inviter = await _uow.Users.GetByIdAsync(currentUserId, cancellationToken);
        var clinic  = await _uow.Clinics.GetByIdAsync(clinicId, cancellationToken);

        await _emailService.SendStaffInvitationEmailAsync(
            invitation.Email,
            clinic?.Name ?? "Clinic",
            invitation.Role.ToString(),
            inviter?.FullName ?? "Clinic Administrator",
            $"/accept-invitation/{invitation.InvitationToken}",
            cancellationToken);

        await _uow.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
