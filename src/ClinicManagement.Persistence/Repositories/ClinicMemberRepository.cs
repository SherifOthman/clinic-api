using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Features.Staff.QueryModels;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class ClinicMemberRepository : Repository<ClinicMember>, IClinicMemberRepository
{
    private readonly DbSet<IdentityUserRole<Guid>> _userRoles;
    private readonly DbSet<Role> _roles;

    public ClinicMemberRepository(ApplicationDbContext context) : base(context)
    {
        _userRoles = context.Set<IdentityUserRole<Guid>>();
        _roles = context.Set<Role>();
    }

    public async Task<ClinicMember?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(m => m.UserId == userId, ct);

    public async Task<ClinicMember?> GetByUserIdIgnoreFiltersAsync(Guid userId, CancellationToken ct = default)
        => await DbSet
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .FirstOrDefaultAsync(m => m.UserId == userId, ct);

    public async Task<ClinicMember?> GetByUserIdWithClinicAsync(Guid userId, CancellationToken ct = default)
        => await DbSet
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .Include(m => m.Clinic)
            .FirstOrDefaultAsync(m => m.UserId == userId, ct);

    public async Task<ClinicMember?> GetByIdWithDoctorInfoAsync(Guid id, CancellationToken ct = default)
        => await DbSet.AsNoTracking().Include(m => m.DoctorInfo).FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<int> CountActiveAsync(CancellationToken ct = default)
        => await DbSet.CountAsync(m => m.IsActive, ct);

    public async Task<int> CountActiveIgnoreFiltersAsync(CancellationToken ct = default)
        => await DbSet
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .CountAsync(m => m.IsActive, ct);

    public async Task<PaginatedResult<StaffListRow>> GetProjectedPageAsync(
        bool? isActive, string? role, string? sortBy, string? sortDirection,
        int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking();

        if (isActive.HasValue)
            query = query.Where(m => m.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(role))
        {
            if (Enum.TryParse<Domain.Enums.ClinicMemberRole>(role, out var roleEnum))
                query = query.Where(m => m.Role == roleEnum);
        }

        var projected = query.Select(m => new
        {
            m.Id,
            UserId = m.UserId ?? Guid.Empty,
            m.IsActive,
            m.JoinedAt,
            FullName = m.Person.FullName,
            Gender = m.Person.Gender,
            ProfileImageUrl = m.Person.ProfileImageUrl,
        });

        var desc = sortDirection.IsDescending();
        projected = sortBy?.Trim().ToLower() switch
        {
            "fullname" => desc
                ? projected.OrderByDescending(m => m.FullName)
                : projected.OrderBy(m => m.FullName),
            "joindate" => desc
                ? projected.OrderByDescending(m => m.JoinedAt)
                : projected.OrderBy(m => m.JoinedAt),
            _ => projected.OrderByDescending(m => m.JoinedAt),
        };

        var pagedRaw = await projected.ToPagedAsync(pageNumber, pageSize, ct);

        var items = pagedRaw.Items.Select(m => new StaffListRow(
            m.Id, m.UserId, m.IsActive, m.JoinedAt,
            m.FullName, m.Gender.ToString(), m.ProfileImageUrl
        )).ToList();

        return PaginatedResult<StaffListRow>.Create(items, pagedRaw.TotalCount, pageNumber, pageSize);
    }

    public async Task<StaffDetailRow?> GetDetailAsync(Guid memberId, CancellationToken ct = default)
    {
        var member = await DbSet.AsNoTracking()
            .Where(m => m.Id == memberId)
            .Select(m => new
            {
                m.Id,
                UserId = m.UserId ?? Guid.Empty,
                m.IsActive,
                m.JoinedAt,
                FullName = m.Person.FullName,
                Gender = m.Person.Gender,
                Email = m.User != null ? m.User.Email : null,
                PhoneNumber = m.User != null ? m.User.PhoneNumber : null,
                ProfileImageUrl = m.Person.ProfileImageUrl,
                DoctorInfo = m.DoctorInfo == null ? null : new DoctorDetailRow(
                    m.DoctorInfo.Id,
                    m.DoctorInfo.Specialization != null ? m.DoctorInfo.Specialization.NameEn : "",
                    m.DoctorInfo.Specialization != null ? m.DoctorInfo.Specialization.NameAr : "",
                    m.DoctorInfo.CanSelfManageSchedule,
                    m.DoctorInfo.AppointmentType.ToString()),
            })
            .FirstOrDefaultAsync(ct);

        if (member is null) return null;

        return new StaffDetailRow(
            member.Id, member.UserId, member.IsActive, member.JoinedAt,
            member.FullName.Trim(), member.Gender.ToString(),
            member.Email, member.PhoneNumber, member.ProfileImageUrl, member.DoctorInfo);
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
}
