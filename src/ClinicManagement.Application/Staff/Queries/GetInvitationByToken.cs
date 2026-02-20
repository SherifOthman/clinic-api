using ClinicManagement.Domain.Repositories;
using MediatR;

namespace ClinicManagement.Application.Staff.Queries;

public record GetInvitationByTokenQuery(string Token) : IRequest<InvitationDto?>;

public record InvitationDto(
    int Id,
    string Email,
    string Role,
    string Token,
    DateTime ExpiresAt,
    DateTime CreatedAt,
    string InvitedByUserName
);

public class GetInvitationByTokenHandler : IRequestHandler<GetInvitationByTokenQuery, InvitationDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetInvitationByTokenHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<InvitationDto?> Handle(GetInvitationByTokenQuery request, CancellationToken cancellationToken)
    {
        var invitation = await _unitOfWork.StaffInvitations.GetByTokenAsync(request.Token, cancellationToken);

        if (invitation == null)
            return null;

        var invitedByUser = await _unitOfWork.Users.GetByIdAsync(invitation.CreatedByUserId, cancellationToken);

        return new InvitationDto(
            invitation.Id,
            invitation.Email,
            invitation.Role,
            invitation.InvitationToken,
            invitation.ExpiresAt,
            invitation.CreatedAt,
            invitedByUser?.FullName ?? "Unknown"
        );
    }
}
