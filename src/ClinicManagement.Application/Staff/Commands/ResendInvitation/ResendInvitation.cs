using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Staff.Commands;

public record ResendInvitationCommand(Guid InvitationId) : IRequest<Result>;

public class ResendInvitationHandler : IRequestHandler<ResendInvitationCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;
    private readonly IClock _clock;

    public ResendInvitationHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IEmailService emailService,
        IClock clock)
    {
        _context = context;
        _currentUserService = currentUserService;
        _emailService = emailService;
        _clock = clock;
    }

    public async Task<Result> Handle(ResendInvitationCommand request, CancellationToken cancellationToken)
    {
        var clinicId = _currentUserService.GetRequiredClinicId();
        var currentUserId = _currentUserService.GetRequiredUserId();

        var invitation = await _context.StaffInvitations
            .Include(si => si.Specialization)
            .FirstOrDefaultAsync(si => si.Id == request.InvitationId, cancellationToken);

        if (invitation == null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Invitation not found");

        if (invitation.ClinicId != clinicId)
            return Result.Failure(ErrorCodes.FORBIDDEN, "You can only resend invitations from your own clinic");

        if (invitation.IsAccepted || invitation.IsCanceled)
            return Result.Failure(ErrorCodes.OPERATION_NOT_ALLOWED, "Cannot resend an accepted or cancelled invitation");

        // Extend expiry by 7 days from now
        invitation.ExpiresAt = _clock.UtcNow.AddDays(7);

        var inviter = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken);
        var invitedBy = inviter?.FullName ?? "Clinic Administrator";

        var clinic = await _context.Clinics
            .FirstOrDefaultAsync(c => c.Id == clinicId, cancellationToken);
        var clinicName = clinic?.Name ?? "Clinic";

        var invitationLink = $"/accept-invitation/{invitation.InvitationToken}";

        await _emailService.SendStaffInvitationEmailAsync(
            invitation.Email,
            clinicName,
            invitation.Role,
            invitedBy,
            invitationLink,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
