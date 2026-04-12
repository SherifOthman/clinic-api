using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Queries;

public record GetInvitationsQuery(
    InvitationStatus? Status = null,
    string? Role = null,
    string? SortBy = null,
    string? SortDirection = null,
    int PageNumber = 1,
    int PageSize = 10
) : PaginatedQuery(PageNumber, PageSize), IRequest<Result<PaginatedResult<InvitationDto>>>;

public record InvitationDto(
    Guid Id,
    string Email,
    string Role,
    string? SpecializationNameEn,
    string? SpecializationNameAr,
    InvitationStatus Status,
    DateTimeOffset InvitedAt,
    DateTimeOffset ExpiresAt,
    string InvitedBy
);
