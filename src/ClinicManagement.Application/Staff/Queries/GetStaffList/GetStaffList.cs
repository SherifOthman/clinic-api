using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Staff.Queries;

public record GetStaffListQuery(string? RoleFilter = null) : IRequest<IEnumerable<StaffDto>>;

public record StaffDto(
    Guid Id,
    Guid UserId,
    string FullName,
    string Email,
    string? PhoneNumber,
    string Role,
    bool IsActive,
    DateTime HireDate
);

public class GetStaffListHandler : IRequestHandler<GetStaffListQuery, IEnumerable<StaffDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<User> _userManager;

    public GetStaffListHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        UserManager<User> userManager)
    {
        _context = context;
        _currentUserService = currentUserService;
        _userManager = userManager;
    }

    public async Task<IEnumerable<StaffDto>> Handle(GetStaffListQuery request, CancellationToken cancellationToken)
    {
        var clinicId = _currentUserService.GetRequiredClinicId();

        var staffList = await _context.Staff
            .Where(s => s.ClinicId == clinicId)
            .Include(s => s.User)
            .ToListAsync(cancellationToken);

        var result = new List<StaffDto>();

        foreach (var staff in staffList)
        {
            if (staff.User == null) continue;

            var userRoles = await _userManager.GetRolesAsync(staff.User);
            var role = userRoles.FirstOrDefault() ?? "Unknown";

            if (!string.IsNullOrEmpty(request.RoleFilter) && role != request.RoleFilter)
                continue;

            result.Add(new StaffDto(
                staff.Id,
                staff.User.Id,
                staff.User.FullName,
                staff.User.Email!,
                staff.User.PhoneNumber,
                role,
                staff.IsActive,
                staff.HireDate
            ));
        }

        return result;
    }
}
