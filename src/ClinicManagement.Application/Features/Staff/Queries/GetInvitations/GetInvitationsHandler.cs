using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Queries;

public class GetInvitationsHandler : IRequestHandler<GetInvitationsQuery, Result<PaginatedResult<InvitationDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetInvitationsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<PaginatedResult<InvitationDto>>> Handle(
        GetInvitationsQuery request, CancellationToken cancellationToken)
    {
        var now    = DateTimeOffset.UtcNow;
        var result = await _uow.Invitations.GetProjectedPageAsync(
            request.Filter,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var items = result.Items.Select(si =>
        {
            var status = si.IsAccepted ? InvitationStatus.Accepted
                : si.IsCanceled        ? InvitationStatus.Canceled
                : si.ExpiresAt <= now  ? InvitationStatus.Expired
                : InvitationStatus.Pending;

            return new InvitationDto(si.Id, si.Email, si.Role,
                si.SpecializationNameEn, si.SpecializationNameAr,
                status, si.CreatedAt, si.ExpiresAt, si.InvitedBy);
        });

        return Result.Success(PaginatedResult<InvitationDto>.Create(items, result.TotalCount, result.PageNumber, result.PageSize));
    }
}
