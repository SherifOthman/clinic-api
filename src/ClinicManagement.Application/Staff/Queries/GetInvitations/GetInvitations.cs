using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Staff.Queries;

public enum InvitationStatus { Pending, Accepted, Canceled, Expired }

public record GetInvitationsQuery(
    InvitationStatus? Status = null,
    int PageNumber = 1,
    int PageSize = 10
) : PaginatedQuery(PageNumber, PageSize), IRequest<Result<PaginatedResult<InvitationDto>>>;

public record InvitationDto(
    Guid Id,
    string Email,
    string Role,
    string? SpecializationName,
    InvitationStatus Status,
    DateTime InvitedAt,
    DateTime ExpiresAt,
    string InvitedBy,
    DateTime? AcceptedAt,
    string? AcceptedBy
);

public class GetInvitationsHandler : IRequestHandler<GetInvitationsQuery, Result<PaginatedResult<InvitationDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetInvitationsHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginatedResult<InvitationDto>>> Handle(GetInvitationsQuery request, CancellationToken cancellationToken)
    {
        var clinicId = _currentUserService.GetRequiredClinicId();
        var now = DateTime.UtcNow;

        var query = _context.StaffInvitations
            .Where(si => si.ClinicId == clinicId && !si.IsDeleted)
            .Include(si => si.CreatedByUser)
            .Include(si => si.AcceptedByUser)
            .Include(si => si.Specialization)
            .AsQueryable();

        if (request.Status.HasValue)
        {
            query = request.Status.Value switch
            {
                InvitationStatus.Pending   => query.Where(si => !si.IsAccepted && !si.IsCanceled && si.ExpiresAt > now),
                InvitationStatus.Accepted  => query.Where(si => si.IsAccepted),
                InvitationStatus.Canceled  => query.Where(si => si.IsCanceled),
                InvitationStatus.Expired   => query.Where(si => !si.IsAccepted && !si.IsCanceled && si.ExpiresAt <= now),
                _ => query
            };
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var invitations = await query
            .OrderByDescending(si => si.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = invitations.Select(si =>
        {
            var status = si.IsAccepted ? InvitationStatus.Accepted
                : si.IsCanceled ? InvitationStatus.Canceled
                : si.ExpiresAt <= now ? InvitationStatus.Expired
                : InvitationStatus.Pending;

            return new InvitationDto(
                si.Id,
                si.Email,
                si.Role,
                si.Specialization?.NameEn,
                status,
                si.CreatedAt,
                si.ExpiresAt,
                si.CreatedByUser.FullName,
                si.AcceptedAt,
                si.AcceptedByUser?.FullName
            );
        });

        return Result<PaginatedResult<InvitationDto>>.Success(
            PaginatedResult<InvitationDto>.Create(items, totalCount, request.PageNumber, request.PageSize)
        );
    }
}
