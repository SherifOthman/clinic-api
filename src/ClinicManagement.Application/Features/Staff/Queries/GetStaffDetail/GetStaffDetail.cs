using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Staff.Queries;

public record GetStaffDetailQuery(Guid StaffId) : IRequest<Result<StaffDetailDto>>;

public record StaffDetailDto(
    Guid Id,
    string FullName,
    string Gender,
    string? Email,
    string? PhoneNumber,
    string JoinDate,
    string? UpdatedAt,
    string? ProfileImageUrl,
    bool IsActive,
    IEnumerable<StaffRoleDto> Roles,
    DoctorDetailDto? DoctorProfile
);

public record DoctorDetailDto(
    Guid DoctorProfileId,
    string SpecializationNameEn,
    string SpecializationNameAr
);

public class GetStaffDetailHandler : IRequestHandler<GetStaffDetailQuery, Result<StaffDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetStaffDetailHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<StaffDetailDto>> Handle(GetStaffDetailQuery request, CancellationToken cancellationToken)
    {
        var staff = await _context.Staff
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.DoctorProfile)
                .ThenInclude(dp => dp!.Specialization)
            .FirstOrDefaultAsync(s => s.Id == request.StaffId, cancellationToken);

        if (staff == null)
            return Result.Failure<StaffDetailDto>(ErrorCodes.NOT_FOUND, "Staff member not found");

        var roles = await _context.UserRoles
            .Where(ur => ur.UserId == staff.UserId)
            .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new StaffRoleDto(r.Name!))
            .ToListAsync(cancellationToken);

        var dto = new StaffDetailDto(
            staff.Id,
            staff.User.FullName,
            staff.User.IsMale ? "Male" : "Female",
            staff.User.Email,
            staff.User.PhoneNumber,
            staff.CreatedAt.ToString("O"),
            staff.UpdatedAt?.ToString("O"),
            staff.User.ProfileImageUrl,
            staff.IsActive,
            roles,
            staff.DoctorProfile == null ? null : new DoctorDetailDto(
                staff.DoctorProfile.Id,
                staff.DoctorProfile.Specialization?.NameEn ?? "",
                staff.DoctorProfile.Specialization?.NameAr ?? ""
            )
        );

        return Result.Success(dto);
    }
}
