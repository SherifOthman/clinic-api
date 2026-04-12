using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Queries;

public class GetInvitationDetailHandler : IRequestHandler<GetInvitationDetailQuery, Result<InvitationDetailDto>>
{
    private readonly IUnitOfWork _uow;

    public GetInvitationDetailHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<InvitationDetailDto>> Handle(
        GetInvitationDetailQuery request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var si  = await _uow.Invitations.GetDetailAsync(request.InvitationId, cancellationToken);

        if (si is null)
            return Result.Failure<InvitationDetailDto>(ErrorCodes.NOT_FOUND, "Invitation not found");

        var status = si.IsAccepted ? InvitationStatus.Accepted
            : si.IsCanceled        ? InvitationStatus.Canceled
            : si.ExpiresAt <= now  ? InvitationStatus.Expired
            : InvitationStatus.Pending;

        return Result.Success(new InvitationDetailDto(
            si.Id, si.Email, si.Role,
            si.SpecializationNameEn, si.SpecializationNameAr,
            status, si.CreatedAt, si.ExpiresAt, si.CreatedByName, si.AcceptedAt, si.AcceptedByName));
    }
}
