using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Features.Staff.QueryModels;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class StaffRepository : Repository<Staff>, IStaffRepository
{
    private readonly DbSet<IdentityUserRole<Guid>> _userRoles;
    private readonly DbSet<Role> _roles;

    public StaffRepository(ApplicationDbContext context) : base(context)
    {
        _userRoles = context.Set<IdentityUserRole<Guid>>();
        _roles     = context.Set<Role>();
    }

    // ── Simple lookups ────────────────────────────────────────────────────────

    public async Task<Staff?> GetByIdWithDoctorProfileAsync(Guid id, CancellationToken ct = default)
        => await DbSet.Include(s => s.DoctorProfile).FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<Staff?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(s => s.UserId == userId, ct);

    public async Task<Staff?> GetByUserIdIgnoreFiltersAsync(Guid userId, CancellationToken ct = default)
        => await DbSet
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .FirstOrDefaultAsync(s => s.UserId == userId, ct);

    public async Task<int> CountActiveIgnoreFiltersAsync(CancellationToken ct = default)
        => await DbSet
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .CountAsync(s => s.IsActive, ct);

    public async Task<int> CountActiveAsync(CancellationToken ct = default)
        => await DbSet.CountAsync(s => s.IsActive, ct);

    // ── Paginated list ────────────────────────────────────────────────────────

    public async Task<PaginatedResult<StaffListRow>> GetProjectedPageAsync(
        bool? isActive,
        string? role,
        string? sortBy,
        string? sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking();

        if (isActive.HasValue)
            query = query.Where(s => s.IsActive == isActive.Value);

        query = ApplyRoleFilter(query, role);

        var projected = query.Select(s => new
        {
            s.Id,
            s.UserId,
            s.IsActive,
            s.CreatedAt,
            FirstName       = s.User.FirstName,
            LastName        = s.User.LastName,
            Gender          = s.User.Gender,
            ProfileImageUrl = s.User.ProfileImageUrl,
        });

        var desc = sortDirection.IsDescending();
        projected = sortBy?.Trim().ToLower() switch
        {
            "fullname" => desc
                ? projected.OrderByDescending(s => s.FirstName).ThenByDescending(s => s.LastName)
                : projected.OrderBy(s => s.FirstName).ThenBy(s => s.LastName),
            "joindate" => desc
                ? projected.OrderByDescending(s => s.CreatedAt)
                : projected.OrderBy(s => s.CreatedAt),
            _          => projected.OrderByDescending(s => s.CreatedAt),
        };

        var pagedRaw = await projected.ToPagedAsync(pageNumber, pageSize, ct);

        var items = pagedRaw.Items.Select(s => new StaffListRow(
            s.Id, s.UserId, s.IsActive, s.CreatedAt,
            s.FirstName, s.LastName, s.Gender.ToString(), s.ProfileImageUrl
        )).ToList();

        return PaginatedResult<StaffListRow>.Create(items, pagedRaw.TotalCount, pageNumber, pageSize);
    }

    // ── Detail ────────────────────────────────────────────────────────────────

    public async Task<StaffDetailRow?> GetDetailAsync(Guid staffId, CancellationToken ct = default)
    {
        var staff = await DbSet.AsNoTracking()
            .Where(s => s.Id == staffId)
            .Select(s => new
            {
                s.Id, s.UserId, s.IsActive, s.CreatedAt,
                FullName        = s.User.FirstName + " " + s.User.LastName,
                Gender          = s.User.Gender,
                s.User.Email,
                s.User.PhoneNumber,
                s.User.ProfileImageUrl,
                DoctorProfile   = s.DoctorProfile == null ? null : new DoctorDetailRow(
                    s.DoctorProfile.Id,
                    s.DoctorProfile.Specialization != null ? s.DoctorProfile.Specialization.NameEn : "",
                    s.DoctorProfile.Specialization != null ? s.DoctorProfile.Specialization.NameAr : ""),
            })
            .FirstOrDefaultAsync(ct);

        if (staff is null) return null;

        return new StaffDetailRow(
            staff.Id, staff.UserId, staff.IsActive, staff.CreatedAt,
            staff.FullName.Trim(), staff.Gender.ToString(),
            staff.Email, staff.PhoneNumber, staff.ProfileImageUrl, staff.DoctorProfile);
    }

    public async Task<List<StaffRoleRow>> GetRolesByUserIdsAsync(
        List<Guid> userIds, CancellationToken ct = default)
    {
        var rows = await (
            from ur in _userRoles
            join r in _roles on ur.RoleId equals r.Id
            where userIds.Contains(ur.UserId)
            select new { ur.UserId, r.Name }
        ).ToListAsync(ct);

        return rows.Select(r => new StaffRoleRow(r.UserId, r.Name!)).ToList();
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private IQueryable<Staff> ApplyRoleFilter(IQueryable<Staff> query, string? role)
    {
        if (string.IsNullOrWhiteSpace(role)) return query;

        var usersWithRole =
            from ur in _userRoles
            join r in _roles on ur.RoleId equals r.Id
            where r.Name == role
            select ur.UserId;

        return query.Where(s => usersWithRole.Contains(s.UserId));
    }
}
