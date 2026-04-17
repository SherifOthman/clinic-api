using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Features.Staff.QueryModels;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Queries;

public class GetStaffListHandler : IRequestHandler<GetStaffListQuery, Result<PaginatedResult<StaffDto>>>
{
    private readonly IUnitOfWork _uow;
    public GetStaffListHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<PaginatedResult<StaffDto>>> Handle(
        GetStaffListQuery request, CancellationToken cancellationToken)
    {
        // Use new Members repository
        var result = await _uow.Members.GetProjectedPageAsync(
            request.IsActive, request.Role,
            request.SortBy, request.SortDirection,
            request.PageNumber, request.PageSize,
            cancellationToken);

        var userIds  = result.Items.Select(s => s.UserId).ToList();
        var roleRows = await _uow.Members.GetRolesByUserIdsAsync(userIds, cancellationToken);
        var rolesMap = roleRows
            .GroupBy(r => r.UserId)
            .ToDictionary(g => g.Key, g => g.Select(r => new StaffRoleDto(r.RoleName)).ToList());

        var dtos = result.Items.Select(s => new StaffDto(
            s.Id,
            $"{s.FirstName} {s.LastName}".Trim(),
            s.Gender,
            s.CreatedAt,
            s.ProfileImageUrl,
            s.IsActive,
            rolesMap.TryGetValue(s.UserId, out var roles) ? roles : []
        ));

        return Result.Success(PaginatedResult<StaffDto>.Create(dtos, result.TotalCount, result.PageNumber, result.PageSize));
    }
}
