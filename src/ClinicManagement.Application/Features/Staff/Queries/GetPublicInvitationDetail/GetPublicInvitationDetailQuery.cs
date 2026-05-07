using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Queries;

/// <summary>
/// Public query — no authentication required.
/// Returns minimal invitation info so the website can show context
/// (clinic name, role) before the invitee fills the registration form.
/// </summary>
public record GetPublicInvitationDetailQuery(string Token)
    : IRequest<Result<PublicInvitationDetailDto>>;

public record PublicInvitationDetailDto(
    string Email,
    string Role,
    string ClinicName,
    bool IsExpired,
    bool IsAccepted,
    string? SpecializationName);
