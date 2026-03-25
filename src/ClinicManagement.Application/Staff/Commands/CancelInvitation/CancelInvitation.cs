using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Staff.Commands;

public record CancelInvitationCommand(Guid InvitationId) : IRequest<Result>;

public class CancelInvitationHandler : IRequestHandler<CancelInvitationCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CancelInvitationHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(CancelInvitationCommand request, CancellationToken cancellationToken)
    {
        var clinicId = _currentUserService.GetRequiredClinicId();

        var invitation = await _context.StaffInvitations
            .FirstOrDefaultAsync(si => si.Id == request.InvitationId, cancellationToken);
        
        if (invitation == null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Invitation not found");

        if (invitation.ClinicId != clinicId)
            return Result.Failure(ErrorCodes.FORBIDDEN, "You can only cancel invitations from your own clinic");

        var cancelResult = invitation.Cancel();
        if (cancelResult.IsFailure)
            return cancelResult;
        
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
