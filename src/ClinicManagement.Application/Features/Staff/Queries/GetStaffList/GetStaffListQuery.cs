using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Models.Filters;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Queries;

public record GetStaffListQuery(
    StaffFilter Filter,
    int PageNumber = 1,
    int PageSize   = 10
) : PaginatedQuery(PageNumber, PageSize), IRequest<Result<PaginatedResult<StaffDto>>>;

public record StaffDto(
    Guid Id,
    string FullName,
    string Gender,
    DateTimeOffset JoinDate,
    string? ProfileImageUrl,
    bool IsActive,
    IEnumerable<StaffRoleDto> Roles
);

public record StaffRoleDto(string Name);
