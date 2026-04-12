using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Branches.Queries;

public record GetBranchesQuery : IRequest<Result<List<BranchDto>>>;

public record BranchDto(
    Guid Id,
    string Name,
    string? AddressLine,
    string? CityNameEn,
    string? CityNameAr,
    string? StateNameEn,
    string? StateNameAr,
    bool IsMainBranch,
    bool IsActive,
    List<BranchPhoneDto> PhoneNumbers
);

public record BranchPhoneDto(string PhoneNumber, string? Label);
