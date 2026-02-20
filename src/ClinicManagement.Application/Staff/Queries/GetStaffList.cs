using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Repositories;
using MediatR;

namespace ClinicManagement.Application.Staff.Queries;

public record GetStaffListQuery(string? RoleFilter = null) : IRequest<IEnumerable<StaffDto>>;

public record StaffDto(
    int Id,
    int UserId,
    string FullName,
    string Email,
    string? PhoneNumber,
    string Role,
    bool IsActive,
    DateTime HireDate
);

public class GetStaffListHandler : IRequestHandler<GetStaffListQuery, IEnumerable<StaffDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetStaffListHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<StaffDto>> Handle(GetStaffListQuery request, CancellationToken cancellationToken)
    {
        var clinicId = _currentUserService.ClinicId;

        if (!clinicId.HasValue)
            throw new UnauthorizedAccessException("User must belong to a clinic");

        var staffList = await _unitOfWork.Staff.GetByClinicIdAsync(clinicId.Value, cancellationToken);

        var result = new List<StaffDto>();

        foreach (var staff in staffList)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(staff.UserId, cancellationToken);
            
            if (user == null) continue;

            var userRoles = await _unitOfWork.Users.GetUserRolesAsync(user.Id, cancellationToken);
            var role = userRoles.FirstOrDefault() ?? "Unknown";

            if (!string.IsNullOrEmpty(request.RoleFilter) && role != request.RoleFilter)
                continue;

            result.Add(new StaffDto(
                staff.Id,
                user.Id,
                user.FullName,
                user.Email,
                user.PhoneNumber,
                role,
                staff.IsActive,
                staff.HireDate
            ));
        }

        return result;
    }
}
