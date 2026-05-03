using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Authentication;

/// <summary>
/// Centralises the two steps shared by every login flow
/// (password login, Google OAuth, token refresh):
///
///   1. ResolveContextAsync  — figures out clinicId / memberId / countryCode
///      from the user's role, so the JWT carries the right claims.
///
///   2. IssueTokenPairAsync  — generates the access token + refresh token
///      and returns the response DTO.
///
/// Keeping this logic here means LoginHandler, GoogleLoginHandler, and
/// RefreshTokenHandler don't each maintain their own copy.
/// </summary>
public interface ITokenIssuer
{
    /// <summary>
    /// Resolves the clinic context (clinicId, memberId, countryCode) for a user.
    /// Returns Failure if the user's staff account is inactive.
    /// </summary>
    Task<Result<TokenContext>> ResolveContextAsync(
        Guid userId,
        IReadOnlyList<string> roles,
        CancellationToken ct = default);

    /// <summary>
    /// Generates an access token + refresh token pair and returns the response DTO.
    /// Does NOT revoke any existing token — callers that need revocation (RefreshTokenHandler)
    /// do it themselves before calling this.
    /// </summary>
    Task<TokenResponseDto> IssueTokenPairAsync(
        User user,
        IReadOnlyList<string> roles,
        TokenContext context,
        CancellationToken ct = default);
}

/// <summary>Clinic / member context resolved from the user's role — used to build JWT claims.</summary>
public record TokenContext(Guid? ClinicId, Guid? MemberId, string? CountryCode)
{
    public static readonly TokenContext Empty = new(null, null, null);
}
