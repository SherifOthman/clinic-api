using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Staff.Queries;

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
    IEnumerable<StaffRoleDto> Roles
);

public record StaffRoleDto(string Name);

public class GetStaffListHandler : IRequestHandler<GetStaffListQuery, Result<PaginatedResult<StaffDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetStaffListHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<PaginatedResult<StaffDto>>> Handle(
        GetStaffListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Staff
            .AsNoTracking()
            .Include(s => s.User)
            .AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(s => s.IsActive == request.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            var usersWithRole =
                from ur in _context.UserRoles
                join r  in _context.Roles on ur.RoleId equals r.Id
                where r.Name == request.Role
                select ur.UserId;

            query = query.Where(s => usersWithRole.Contains(s.UserId));
        }

        var descending = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        query = request.SortBy?.ToLower() switch
        {
            "name"     => descending
                            ? query.OrderByDescending(s => s.User.FirstName).ThenByDescending(s => s.User.LastName)
                            : query.OrderBy(s => s.User.FirstName).ThenBy(s => s.User.LastName),
            "joindate" => descending
                            ? query.OrderByDescending(s => s.CreatedAt)
                            : query.OrderBy(s => s.CreatedAt),
            _          => query.OrderByDescending(s => s.CreatedAt),
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var page = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var userIds = page.Select(s => s.UserId).ToList();

        var userRoles = await (
            from ur in _context.UserRoles
            join r  in _context.Roles on ur.RoleId equals r.Id
            where userIds.Contains(ur.UserId)
            select new { ur.UserId, r.Name }
        ).ToListAsync(cancellationToken);

        var rolesByUser = userRoles
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.Select(x => new StaffRoleDto(x.Name!)).ToList());

        var items = page.Select(s => new StaffDto(
            s.Id,
            s.User.FullName,
            s.User.IsMale ? "Male" : "Female",
            s.CreatedAt,
            s.User.ProfileImageUrl,
            s.IsActive,
            rolesByUser.TryGetValue(s.UserId, out var roles) ? roles : []
        ));

        return Result<PaginatedResult<StaffDto>>.Success(
            PaginatedResult<StaffDto>.Create(items, totalCount, request.PageNumber, request.PageSize));
    }
}
