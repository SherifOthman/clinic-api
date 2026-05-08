using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Models.Filters;
using ClinicManagement.Application.Features.Staff.Queries;
using ClinicManagement.Application.Features.Staff.QueryModels;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Persistence.Security;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class InvitationRepository : Repository<StaffInvitation>, IInvitationRepository
{
    // Adding a new sortable column = one new line here, no method changes.
    private static readonly Dictionary<string, System.Linq.Expressions.Expression<Func<StaffInvitation, object>>> SortMap = new()
    {
        ["email"]     = si => si.Email,
        ["invitedat"] = si => si.CreatedAt,
        ["expiresat"] = si => si.ExpiresAt,
    };

    public InvitationRepository(ApplicationDbContext context) : base(context) { }

    public async Task<StaffInvitation?> GetByIdWithSpecializationAsync(Guid id, CancellationToken ct = default)
        => await DbSet.Include(si => si.Specialization).FirstOrDefaultAsync(si => si.Id == id, ct);

    public async Task<StaffInvitation?> GetByTokenAsync(string token, CancellationToken ct = default)
        => await TenantGuard.AsSystemQuery(DbSet)
            .FirstOrDefaultAsync(si => si.InvitationToken == token && !si.IsDeleted, ct);

    public async Task<StaffInvitation?> GetByTokenWithSpecializationAsync(string token, CancellationToken ct = default)
        => await TenantGuard.AsSystemQuery(DbSet)
            .Include(si => si.Specialization)
            .FirstOrDefaultAsync(si => si.InvitationToken == token && !si.IsDeleted, ct);

    public async Task<int> CountPendingAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        return await DbSet.CountAsync(
            si => !si.IsAccepted && !si.IsCanceled && si.ExpiresAt > now, ct);
    }

    public async Task<PaginatedResult<InvitationListRow>> GetProjectedPageAsync(
        InvitationFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var query = DbSet.AsNoTracking();

        if (filter.Status.HasValue)
            query = filter.Status.Value switch
            {
                InvitationStatus.Pending  => query.Where(si => !si.IsAccepted && !si.IsCanceled && si.ExpiresAt > now),
                InvitationStatus.Accepted => query.Where(si => si.IsAccepted),
                InvitationStatus.Canceled => query.Where(si => si.IsCanceled),
                InvitationStatus.Expired  => query.Where(si => !si.IsAccepted && !si.IsCanceled && si.ExpiresAt <= now),
                _ => query
            };

        if (!string.IsNullOrWhiteSpace(filter.Role) &&
            Enum.TryParse<ClinicMemberRole>(filter.Role, ignoreCase: true, out var roleEnum))
            query = query.Where(si => si.Role == roleEnum);

        var desc = filter.SortDirection.IsDescending();        query = SortMap.TryGetValue(filter.SortBy?.Trim().ToLower() ?? "", out var sortExpr)
            ? (desc ? query.OrderByDescending(sortExpr) : query.OrderBy(sortExpr))
            : query.OrderByDescending(si => si.CreatedAt);

        return await query
            .Select(si => new InvitationListRow(
                si.Id, si.Email, si.Role.ToString(),
                si.Specialization != null ? si.Specialization.NameEn : null,
                si.Specialization != null ? si.Specialization.NameAr : null,
                si.CreatedAt, si.ExpiresAt, si.IsAccepted, si.IsCanceled,
                (si.CreatedByUser.FullName).Trim()))
            .ToPagedAsync(pageNumber, pageSize, ct);
    }

    public async Task<InvitationDetailRow?> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        var si = await DbSet.AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new
            {
                x.Id,
                x.Email,
                x.Role,
                x.CreatedAt,
                x.ExpiresAt,
                x.IsAccepted,
                x.IsCanceled,
                x.AcceptedAt,
                SpecializationNameEn = x.Specialization != null ? x.Specialization.NameEn : null,
                SpecializationNameAr = x.Specialization != null ? x.Specialization.NameAr : null,
                CreatedByName = x.CreatedByUser.FullName,
                AcceptedByName = x.AcceptedByUser != null
                    ? x.AcceptedByUser.FullName
                    : null,
            })
            .FirstOrDefaultAsync(ct);

        if (si is null) return null;

        return new InvitationDetailRow(
            si.Id, si.Email, si.Role.ToString(),
            si.SpecializationNameEn, si.SpecializationNameAr,
            si.CreatedAt, si.ExpiresAt, si.IsAccepted, si.IsCanceled,
            si.AcceptedAt, si.CreatedByName.Trim(), si.AcceptedByName?.Trim());
    }
}
