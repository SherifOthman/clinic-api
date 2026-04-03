using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Staff.Queries;

public enum InvitationStatus { Pending, Accepted, Canceled, Expired }

public record GetInvitationsQuery(
    InvitationStatus? Status = null,
    string? Role = null,
    string? SortBy = null,
    string? SortDirection = null,
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
    string InvitedBy
);

public class GetInvitationsHandler : IRequestHandler<GetInvitationsQuery, Result<PaginatedResult<InvitationDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetInvitationsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedResult<InvitationDto>>> Handle(GetInvitationsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        // ClinicId filter applied automatically via global named filter
        var query = _context.StaffInvitations
            .Where(si => !si.IsDeleted)
            .Include(si => si.CreatedByUser)
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

        if (!string.IsNullOrWhiteSpace(request.Role))
            query = query.Where(si => si.Role == request.Role);

        var totalCount = await query.CountAsync(cancellationToken);

        // Sorting
        var descending = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = request.SortBy?.ToLower() switch
        {
            "email"     => descending ? query.OrderByDescending(si => si.Email) : query.OrderBy(si => si.Email),
            "invitedat" => descending ? query.OrderByDescending(si => si.CreatedAt) : query.OrderBy(si => si.CreatedAt),
            _           => query.OrderByDescending(si => si.CreatedAt),
        };

        var invitations = await query
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
                si.CreatedByUser.FullName
            );
        });

        return Result<PaginatedResult<InvitationDto>>.Success(
            PaginatedResult<InvitationDto>.Create(items, totalCount, request.PageNumber, request.PageSize)
        );
    }
}
