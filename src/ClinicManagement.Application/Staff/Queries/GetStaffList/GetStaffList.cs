using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StaffEntity = ClinicManagement.Domain.Entities.Staff;

namespace ClinicManagement.Application.Staff.Queries;

public record GetStaffListQuery(
    string? Role = null,
    bool? IsActive = null,
    string? SortBy = null,
    string? SortDirection = null,
    int PageNumber = 1,
    int PageSize = 10)
    : PaginatedQuery(PageNumber, PageSize), IRequest<Result<PaginatedResult<StaffDto>>>;

public record StaffDto(
    Guid Id,
    string FullName,
    string Gender,
    DateTime JoinDate,
    string? ProfileImageUrl,
    bool IsActive,
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
        var query = _context.Staff
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.DoctorProfile)
                .ThenInclude(dp => dp!.Specialization)
            .AsQueryable();

        // 🔹 Filter: IsActive
        if (request.IsActive.HasValue)
            query = query.Where(s => s.IsActive == request.IsActive.Value);

        // 🔹 Filter: Role (JOIN-based)
        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            query = query.Where(s =>
                _context.UserRoles
                    .Join(_context.Roles,
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => new { ur.UserId, r.Name })
                    .Any(x => x.UserId == s.UserId && x.Name == request.Role));
        }

        // 🔹 Sorting
        var descending = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        query = request.SortBy?.ToLower() switch
        {
            "name" => descending
                ? query.OrderByDescending(s => s.User.FullName)
                : query.OrderBy(s => s.User.FullName),

            "joindate" => descending
                ? query.OrderByDescending(s => s.CreatedAt)
                : query.OrderBy(s => s.CreatedAt),

            _ => query.OrderByDescending(s => s.CreatedAt),
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var staffList = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // 🔹 Roles query (single batch)
        var userIds = staffList.Select(s => s.UserId).ToList();

        var roles = await _context.UserRoles
            .Where(ur => userIds.Contains(ur.UserId))
            .Join(_context.Roles,
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => new { ur.UserId, r.Name })
            .ToListAsync(cancellationToken);

        var rolesByUser = roles
            .GroupBy(x => x.UserId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => new StaffRoleDto(x.Name!)).ToList()
            );

        var items = staffList.Select(s => new StaffDto(
            s.Id,
            s.User.FullName,
            s.User.IsMale ? "Male" : "Female",
            s.CreatedAt,
            s.User.ProfileImageUrl,
            s.IsActive,
            rolesByUser.TryGetValue(s.UserId, out var r) ? r : [],
            s.DoctorProfile == null ? null : new DoctorInfoDto(
                s.DoctorProfile.Id,
                s.DoctorProfile.Specialization?.NameEn ?? "",
                s.DoctorProfile.Specialization?.NameAr ?? "",
                s.DoctorProfile.Specialization?.DescriptionEn,
                s.DoctorProfile.Specialization?.DescriptionAr
            )
        ));

        return Result<PaginatedResult<StaffDto>>.Success(
            PaginatedResult<StaffDto>.Create(items, totalCount, request.PageNumber, request.PageSize)
        );
    }
}
