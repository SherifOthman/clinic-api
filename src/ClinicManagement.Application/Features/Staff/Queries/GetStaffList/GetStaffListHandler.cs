using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Features.Staff.QueryModels;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Queries;

public class GetStaffListHandler : IRequestHandler<GetStaffListQuery, Result<PaginatedResult<StaffDto>>>
{
    private readonly IClinicMemberRepository _members;

    public GetStaffListHandler(IClinicMemberRepository members) => _members = members;

    public async Task<Result<PaginatedResult<StaffDto>>> Handle(
        GetStaffListQuery request, CancellationToken cancellationToken)
    {
        var result = await _members.GetProjectedPageAsync(
            request.Filter, request.PageNumber, request.PageSize, cancellationToken);

        var userIds  = result.Items.Select(s => s.UserId).ToList();
        var roleRows = await _members.GetRolesByUserIdsAsync(userIds, cancellationToken);
        var rolesMap = roleRows
            .GroupBy(r => r.UserId)
            .ToDictionary(g => g.Key, g => g.Select(r => new StaffRoleDto(r.RoleName)).ToList());

        var dtos = result.Items.Select(s => new StaffDto(
            s.Id,
            s.FullName,
            s.Gender,
            s.CreatedAt,
            s.ProfileImageUrl,
            s.IsActive,
            rolesMap.TryGetValue(s.UserId, out var roles) ? roles : []
        ));

        return Result.Success(PaginatedResult<StaffDto>.Create(dtos, result.TotalCount, result.PageNumber, result.PageSize));
    }
}
