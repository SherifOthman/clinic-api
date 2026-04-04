using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Staff.Queries;

public record GetInvitationDetailQuery(Guid InvitationId) : IRequest<Result<InvitationDetailDto>>;

public record InvitationDetailDto(
    Guid Id,
    string Email,
    string Role,
    string? SpecializationName,
    InvitationStatus Status,
    string InvitedAt,
    string ExpiresAt,
    string InvitedBy,
    string? AcceptedAt,
    string? AcceptedBy
);

public class GetInvitationDetailHandler : IRequestHandler<GetInvitationDetailQuery, Result<InvitationDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetInvitationDetailHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<InvitationDetailDto>> Handle(GetInvitationDetailQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var si = await _context.StaffInvitations
            .AsNoTracking()
            .Include(x => x.CreatedByUser)
            .Include(x => x.AcceptedByUser)
            .Include(x => x.Specialization)
            .FirstOrDefaultAsync(x => x.Id == request.InvitationId && !x.IsDeleted, cancellationToken);

        if (si == null)
            return Result.Failure<InvitationDetailDto>(ErrorCodes.NOT_FOUND, "Invitation not found");

        var status = si.IsAccepted ? InvitationStatus.Accepted
            : si.IsCanceled ? InvitationStatus.Canceled
            : si.ExpiresAt <= now ? InvitationStatus.Expired
            : InvitationStatus.Pending;

        var dto = new InvitationDetailDto(
            si.Id,
            si.Email,
            si.Role,
            si.Specialization?.NameEn,
            status,
            si.CreatedAt.ToString("O"),
            si.ExpiresAt.ToString("O"),
            si.CreatedByUser.FullName,
            si.AcceptedAt?.ToString("O"),
            si.AcceptedByUser?.FullName
        );

        return Result.Success(dto);
    }
}
