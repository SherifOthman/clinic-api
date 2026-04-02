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
    IEnumerable<string> Roles,
    DoctorInfoDto? DoctorInfo
);

public record DoctorInfoDto(
    Guid DoctorProfileId,
    string NameEn,
    string NameAr,
    string? DescriptionEn,
    string? DescriptionAr
);

public class GetStaffListHandler
    : IRequestHandler<GetStaffListQuery, Result<PaginatedResult<StaffDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetStaffListHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginatedResult<StaffDto>>> Handle(
        GetStaffListQuery request,
        CancellationToken cancellationToken)
    {
        var clinicId = _currentUserService.GetRequiredClinicId();

        var query = _context.Staff
            .AsNoTracking()
            .Where(s => s.ClinicId == clinicId);

        // ✅ Simple role filter using navigation
        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            query = query.Where(s =>
                s.User.Roles.Any(r => r.Name == request.Role));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new StaffDto(
                s.Id,
                s.User.FullName,
                s.User.IsMale ? "Male" : "Female",
                s.CreatedAt,
                s.User.ProfileImageUrl,
                s.User.Roles.Select(r => r.Name!),
                s.DoctorProfile == null
                    ? null
                    : new DoctorInfoDto(
                        s.DoctorProfile.Id,
                        s.DoctorProfile.Specialization!.NameEn,
                        s.DoctorProfile.Specialization.NameAr,
                        s.DoctorProfile.Specialization.DescriptionEn,
                        s.DoctorProfile.Specialization.DescriptionAr
                    )
            ))
            .ToListAsync(cancellationToken);

        return Result<PaginatedResult<StaffDto>>.Success(
            PaginatedResult<StaffDto>.Create(
                items,
                totalCount,
                request.PageNumber,
                request.PageSize
            )
        );
    }
}
