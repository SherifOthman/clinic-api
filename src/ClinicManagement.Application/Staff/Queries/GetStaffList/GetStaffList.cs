using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StaffEntity = ClinicManagement.Domain.Entities.Staff;

namespace ClinicManagement.Application.Staff.Queries;

public record GetStaffListQuery(string? Role = null, int PageNumber = 1, int PageSize = 10)
    : PaginatedQuery(PageNumber, PageSize), IRequest<Result<PaginatedResult<StaffDto>>>;

public record StaffDto(
    Guid Id,
    string FullName,
    string Gender,
    DateTime JoinDate,
    string? ProfileImageUrl,
    IEnumerable<StaffRoleDto> Roles,
    DoctorInfoDto? DoctorInfo
);

public record StaffRoleDto(string Name);

public record DoctorInfoDto(
    Guid DoctorProfileId,
    string NameEn,
    string NameAr,
    string? DescriptionEn,
    string? DescriptionAr
);

public class GetStaffListHandler : IRequestHandler<GetStaffListQuery, Result<PaginatedResult<StaffDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetStaffListHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginatedResult<StaffDto>>> Handle(
        GetStaffListQuery request,
        CancellationToken cancellationToken)
    {
        // ClinicId filter applied automatically via global named filter
        var baseQuery = _context.Staff
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.DoctorProfile)
                .ThenInclude(dp => dp!.Specialization);

        // Role filter via UserRoles join (EF-translatable)
        IQueryable<StaffEntity> filteredQuery = baseQuery;
        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            var roleName = request.Role;
            var usersWithRole = _context.UserRoles
                .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
                .Where(x => x.Name == roleName)
                .Select(x => x.UserId);

            filteredQuery = baseQuery.Where(s => usersWithRole.Contains(s.UserId));
        }

        var totalCount = await filteredQuery.CountAsync(cancellationToken);

        var staffList = await filteredQuery
            .OrderByDescending(s => s.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Load roles for fetched users in one query
        var userIds = staffList.Select(s => s.UserId).ToList();

        var userRoles = await _context.UserRoles
            .Where(ur => userIds.Contains(ur.UserId))
            .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
            .ToListAsync(cancellationToken);

        var rolesByUser = userRoles
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.Select(x => new StaffRoleDto(x.Name!)).ToList());

        var items = staffList.Select(s => new StaffDto(
            s.Id,
            s.User.FullName,
            s.User.IsMale ? "Male" : "Female",
            s.CreatedAt,
            s.User.ProfileImageUrl,
            rolesByUser.TryGetValue(s.UserId, out var roles) ? roles : [],
            s.DoctorProfile == null ? null : new DoctorInfoDto(
                s.DoctorProfile.Id,
                s.DoctorProfile.Specialization?.NameEn ?? string.Empty,
                s.DoctorProfile.Specialization?.NameAr ?? string.Empty,
                s.DoctorProfile.Specialization?.DescriptionEn,
                s.DoctorProfile.Specialization?.DescriptionAr
            )
        ));

        return Result<PaginatedResult<StaffDto>>.Success(
            PaginatedResult<StaffDto>.Create(items, totalCount, request.PageNumber, request.PageSize)
        );
    }
}
