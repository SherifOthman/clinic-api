namespace ClinicManagement.Application.Features.Staff.Dtos;

/// <summary>Response returned after successfully sending a staff invitation.</summary>
public record InviteStaffResponseDto(Guid InvitationId, string Token, DateTimeOffset ExpiresAt);
