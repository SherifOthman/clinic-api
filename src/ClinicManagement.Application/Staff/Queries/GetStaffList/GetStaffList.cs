using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Staff.Queries;

public record GetStaffListQuery(string? Role = null, int PageNumber = 1, int PageSize = 10)
    : PaginatedQuery(PageNumber, PageSize), IRequest<Result<PaginatedResult<StaffDto>>>;

public record StaffDto(
    Guid Id,
    string FullName,
    string? Gender,
    string Role,
    DateTime JoinDate,
    string? ProfileImageUrl,
    DoctorInfoDto? DoctorInfo
);

public record DoctorInfoDto(
    Guid DoctorProfileId,
    int? YearsOfExperience,
    string? LicenseNumber,
    string? Bio,
    IEnumerable<SpecializationDto> Specializations
);

public record SpecializationDto(
    Guid Id,
    string NameEn,
    string NameAr,
    bool IsPrimary
);

public class GetStaffListHandler : IRequestHandler<GetStaffListQuery, Result<PaginatedResult<StaffDto>>>
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

    public async Task<Result<PaginatedResult<StaffDto>>> Handle(GetStaffListQuery request, CancellationToken cancellationToken)
    {
        var clinicId = _currentUserService.GetRequiredClinicId();

        var staffList = await _context.Staff
            .Where(s => s.ClinicId == clinicId)
            .Include(s => s.User)
            .Include(s => s.DoctorProfile)
                .ThenInclude(dp => dp!.DoctorSpecializations)
                    .ThenInclude(ds => ds.Specialization)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

        var result = new List<(Domain.Entities.Staff Staff, string Role)>();

        foreach (var staff in staffList)
        {
            if (staff.User == null) continue;

            var roles = await _userManager.GetRolesAsync(staff.User);
            var role = roles.FirstOrDefault() ?? "Unknown";

            if (!string.IsNullOrEmpty(request.Role) && role != request.Role)
                continue;

            result.Add((staff, role));
        }

        var totalCount = result.Count;

        var items = result
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new StaffDto(
                x.Staff.Id,
                x.Staff.User.FullName,
                x.Staff.User.IsMale ? (x.Staff.User.IsMale ? "Male" : "Female") : null,
                x.Role,
                x.Staff.CreatedAt,
                x.Staff.User.ProfileImageUrl,
                x.Staff.DoctorProfile == null ? null : new DoctorInfoDto(
                    x.Staff.DoctorProfile.Id,
                    x.Staff.DoctorProfile.YearsOfExperience,
                    x.Staff.DoctorProfile.LicenseNumber,
                    x.Staff.DoctorProfile.Bio,
                    x.Staff.DoctorProfile.DoctorSpecializations.Select(ds => new SpecializationDto(
                        ds.Specialization.Id,
                        ds.Specialization.NameEn,
                        ds.Specialization.NameAr,
                        ds.IsPrimary
                    ))
                )
            ));

        return Result<PaginatedResult<StaffDto>>.Success(
            PaginatedResult<StaffDto>.Create(items, totalCount, request.PageNumber, request.PageSize)
        );
    }
}
