using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class CancelInvitationHandler : IRequestHandler<CancelInvitationCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;

    public CancelInvitationHandler(IUnitOfWork uow, ICurrentUserService currentUserService)
    {
        _uow                = uow;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(CancelInvitationCommand request, CancellationToken cancellationToken)
    {
        var clinicId   = _currentUserService.GetRequiredClinicId();
        var invitation = await _uow.Invitations.GetByIdAsync(request.InvitationId, cancellationToken);

        if (invitation is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Invitation not found");

        if (invitation.ClinicId != clinicId)
            return Result.Failure(ErrorCodes.FORBIDDEN, "You can only cancel invitations from your own clinic");

        var cancelResult = invitation.Cancel();
        if (cancelResult.IsFailure) return cancelResult;

        await _uow.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
