using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Queries;

public class GetPublicInvitationDetailHandler
    : IRequestHandler<GetPublicInvitationDetailQuery, Result<PublicInvitationDetailDto>>
{
    private readonly IInvitationRepository _invitations;
    private readonly IClinicRepository     _clinics;

    public GetPublicInvitationDetailHandler(IInvitationRepository invitations, IClinicRepository clinics)
    {
        _invitations = invitations;
        _clinics     = clinics;
    }

    public async Task<Result<PublicInvitationDetailDto>> Handle(
        GetPublicInvitationDetailQuery request, CancellationToken cancellationToken)
    {
        var invitation = await _invitations.GetByTokenWithSpecializationAsync(request.Token, cancellationToken);

        if (invitation is null)
            return Result.Failure<PublicInvitationDetailDto>(ErrorCodes.NOT_FOUND, "Invitation not found");

        var clinic = await _clinics.GetByIdAsync(invitation.ClinicId, cancellationToken);

        var isExpired = !invitation.IsAccepted && !invitation.IsCanceled
                        && invitation.ExpiresAt <= DateTimeOffset.UtcNow;

        return Result.Success(new PublicInvitationDetailDto(
            invitation.Email,
            invitation.Role.ToString(),
            clinic?.Name ?? "Clinic",
            isExpired,
            invitation.IsAccepted,
            invitation.Specialization?.NameEn));
    }
}
