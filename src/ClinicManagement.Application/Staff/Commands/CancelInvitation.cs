using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Repositories;
using MediatR;

namespace ClinicManagement.Application.Staff.Commands;

public record CancelInvitationCommand(int InvitationId) : IRequest<Result>;

public class CancelInvitationHandler : IRequestHandler<CancelInvitationCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CancelInvitationHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(CancelInvitationCommand request, CancellationToken cancellationToken)
    {
        // Policy "RequireClinic" ensures user is authenticated and has clinicId claim
        var clinicId = _currentUserService.ClinicId!.Value;

        // Get the invitation
        var invitation = await _unitOfWork.StaffInvitations.GetByIdAsync(request.InvitationId, cancellationToken);
        
        if (invitation == null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Invitation not found");

        // Verify the invitation belongs to the user's clinic
        if (invitation.ClinicId != clinicId)
            return Result.Failure(ErrorCodes.FORBIDDEN, "You can only cancel invitations from your own clinic");

        // Check if invitation is already accepted or canceled
        if (invitation.IsAccepted)
            return Result.Failure(ErrorCodes.INVITATION_ALREADY_ACCEPTED, "Cannot cancel an invitation that has already been accepted");

        if (invitation.IsCanceled)
            return Result.Failure(ErrorCodes.INVITATION_ALREADY_CANCELED, "Invitation is already canceled");

        // Check if invitation has expired
        if (invitation.ExpiresAt < DateTime.UtcNow)
            return Result.Failure(ErrorCodes.INVITATION_EXPIRED, "Cannot cancel an expired invitation");

        // Cancel the invitation
        await _unitOfWork.StaffInvitations.CancelInvitationAsync(request.InvitationId, cancellationToken);

        return Result.Success();
    }
}
