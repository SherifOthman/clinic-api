using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Infrastructure.Services;

/// <inheritdoc cref="ITokenIssuer"/>
public sealed class TokenIssuer : ITokenIssuer
{
    private readonly IUnitOfWork _uow;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokenService;

    public TokenIssuer(
        IUnitOfWork uow,
        ITokenService tokenService,
        IRefreshTokenService refreshTokenService)
    {
        _uow                 = uow;
        _tokenService        = tokenService;
        _refreshTokenService = refreshTokenService;
    }

    // ── Context resolution ────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<Result<TokenContext>> ResolveContextAsync(
        Guid userId,
        IReadOnlyList<string> roles,
        CancellationToken ct = default)
    {
        if (roles.Contains(UserRoles.SuperAdmin))
            return Result.Success(TokenContext.Empty);

        if (roles.Contains(UserRoles.ClinicOwner))
            return await ResolveOwnerContextAsync(userId, ct);

        return await ResolveStaffContextAsync(userId, ct);
    }

    private async Task<Result<TokenContext>> ResolveOwnerContextAsync(Guid userId, CancellationToken ct)
    {
        var clinic = await _uow.Clinics.GetByOwnerIdAsync(userId, ct);
        if (clinic is null)
            return Result.Success(TokenContext.Empty);

        // Owner may also be a doctor — member record carries MemberId for the JWT.
        var member = await _uow.Members.GetByUserIdWithClinicAsync(userId, ct);
        return Result.Success(new TokenContext(clinic.Id, member?.Id, clinic.CountryCode));
    }

    private async Task<Result<TokenContext>> ResolveStaffContextAsync(Guid userId, CancellationToken ct)
    {
        var member = await _uow.Members.GetByUserIdWithClinicAsync(userId, ct);
        if (member is null)
            return Result.Success(TokenContext.Empty);

        if (!member.IsActive)
            return Result.Failure<TokenContext>(
                ErrorCodes.STAFF_INACTIVE,
                "Your account has been deactivated. Please contact your clinic owner.");

        return Result.Success(new TokenContext(member.ClinicId, member.Id, member.Clinic.CountryCode));
    }

    // ── Token pair issuance ───────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<TokenResponseDto> IssueTokenPairAsync(
        User user,
        IReadOnlyList<string> roles,
        TokenContext context,
        CancellationToken ct = default)
    {
        var accessToken  = _tokenService.GenerateAccessToken(
            user, roles, context.MemberId, context.ClinicId, context.CountryCode);

        var rawRefreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id, null, ct);

        return new TokenResponseDto(accessToken, rawRefreshToken);
    }
}
