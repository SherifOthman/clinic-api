using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Queries;

public record GetInvitationDetailQuery(Guid InvitationId) : IRequest<Result<InvitationDetailDto>>;

public record InvitationDetailDto(
    Guid Id,
    string Email,
    string Role,
    string? SpecializationNameEn,
    string? SpecializationNameAr,
    InvitationStatus Status,
    DateTimeOffset InvitedAt,
    DateTimeOffset ExpiresAt,
    string InvitedBy,
    DateTimeOffset? AcceptedAt,
    string? AcceptedBy
);
