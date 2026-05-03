using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Models.Filters;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Queries;

public record GetInvitationsQuery(
    InvitationFilter Filter,
    int PageNumber = 1,
    int PageSize   = 10
) : PaginatedQuery(PageNumber, PageSize), IRequest<Result<PaginatedResult<InvitationDto>>>;

public record InvitationDto(
    Guid Id,
    string Email,
    string Role,
    string? SpecializationNameEn,
    string? SpecializationNameAr,
    Domain.Enums.InvitationStatus Status,
    DateTimeOffset InvitedAt,
    DateTimeOffset ExpiresAt,
    string InvitedBy
);
