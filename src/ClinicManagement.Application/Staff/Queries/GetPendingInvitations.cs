using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Repositories;
using MediatR;

namespace ClinicManagement.Application.Staff.Queries;

public record GetPendingInvitationsQuery : IRequest<IEnumerable<PendingInvitationDto>>;

public record PendingInvitationDto(
    int Id,
    string Email,
    string Role,
    string Token,
    DateTime ExpiresAt,
    DateTime CreatedAt,
    string InvitedByUserName
);

public class GetPendingInvitationsHandler : IRequestHandler<GetPendingInvitationsQuery, IEnumerable<PendingInvitationDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetPendingInvitationsHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<PendingInvitationDto>> Handle(GetPendingInvitationsQuery request, CancellationToken cancellationToken)
    {
        var clinicId = _currentUserService.ClinicId;

        if (!clinicId.HasValue)
            throw new UnauthorizedAccessException("User must belong to a clinic");

        var invitations = await _unitOfWork.StaffInvitations.GetPendingByClinicIdAsync(clinicId.Value, cancellationToken);

        var result = new List<PendingInvitationDto>();

        foreach (var invitation in invitations)
        {
            var invitedByUser = await _unitOfWork.Users.GetByIdAsync(invitation.CreatedByUserId, cancellationToken);
            
            result.Add(new PendingInvitationDto(
                invitation.Id,
                invitation.Email,
                invitation.Role,
                invitation.InvitationToken,
                invitation.ExpiresAt,
                invitation.CreatedAt,
                invitedByUser?.FullName ?? "Unknown"
            ));
        }

        return result;
    }
}
