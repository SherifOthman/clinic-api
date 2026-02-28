using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Staff.Queries;

public record GetPendingInvitationsQuery : IRequest<IEnumerable<PendingInvitationDto>>;

public record PendingInvitationDto(
    Guid Id,
    string Email,
    string Role,
    string Token,
    DateTime ExpiresAt,
    DateTime CreatedAt,
    string InvitedByUserName
);

public class GetPendingInvitationsHandler : IRequestHandler<GetPendingInvitationsQuery, IEnumerable<PendingInvitationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetPendingInvitationsHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<PendingInvitationDto>> Handle(GetPendingInvitationsQuery request, CancellationToken cancellationToken)
    {
        var clinicId = _currentUserService.GetRequiredClinicId();
        var now = DateTime.UtcNow;

        var invitations = await _context.StaffInvitations
            .Where(si => si.ClinicId == clinicId && 
                        !si.IsAccepted && 
                        !si.IsCancelled && 
                        si.ExpiresAt > now)
            .Include(si => si.CreatedByUser)
            .ToListAsync(cancellationToken);

        return invitations.Select(invitation => new PendingInvitationDto(
            invitation.Id,
            invitation.Email,
            invitation.Role,
            invitation.InvitationToken,
            invitation.ExpiresAt,
            invitation.CreatedAt,
            invitation.CreatedByUser?.FullName ?? "Unknown"
        ));
    }
}
