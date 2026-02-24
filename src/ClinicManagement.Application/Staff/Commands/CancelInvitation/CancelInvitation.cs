using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Repositories;
using MediatR;

namespace ClinicManagement.Application.Staff.Commands;

public record CancelInvitationCommand(Guid InvitationId) : IRequest<Result>;

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
        var clinicId = _currentUserService.GetRequiredClinicId();

        var invitation = await _unitOfWork.StaffInvitations.GetByIdAsync(request.InvitationId, cancellationToken);
        
        if (invitation == null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Invitation not found");

        if (invitation.ClinicId != clinicId)
            return Result.Failure(ErrorCodes.FORBIDDEN, "You can only cancel invitations from your own clinic");

        var cancelResult = invitation.Cancel();
        if (cancelResult.IsFailure)
            return cancelResult;
        
        await _unitOfWork.StaffInvitations.UpdateAsync(invitation, cancellationToken);

        return Result.Success();
    }
}
