using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Infrastructure.Services;

public sealed class TokenIssuer : ITokenIssuer
{
    private readonly IClinicRepository       _clinics;
    private readonly IClinicMemberRepository _members;
    private readonly ITokenService           _tokenService;
    private readonly IRefreshTokenService    _refreshTokenService;

    public TokenIssuer(
        IClinicRepository clinics,
        IClinicMemberRepository members,
        ITokenService tokenService,
        IRefreshTokenService refreshTokenService)
    {
        _clinics             = clinics;
        _members             = members;
        _tokenService        = tokenService;
        _refreshTokenService = refreshTokenService;
    }

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
        var clinic = await _clinics.GetByOwnerIdAsync(userId, ct);
        if (clinic is null)
            return Result.Success(TokenContext.Empty);

        var member = await _members.GetByUserIdWithClinicAsync(userId, ct);
        return Result.Success(new TokenContext(clinic.Id, member?.Id, clinic.CountryCode));
    }

    private async Task<Result<TokenContext>> ResolveStaffContextAsync(Guid userId, CancellationToken ct)
    {
        var member = await _members.GetByUserIdWithClinicAsync(userId, ct);
        if (member is null)
            return Result.Success(TokenContext.Empty);

        if (!member.IsActive)
            return Result.Failure<TokenContext>(
                ErrorCodes.STAFF_INACTIVE,
                "Your account has been deactivated. Please contact your clinic owner.");

        return Result.Success(new TokenContext(member.ClinicId, member.Id, member.Clinic.CountryCode));
    }

    public async Task<TokenResponseDto> IssueTokenPairAsync(
        User user,
        IReadOnlyList<string> roles,
        TokenContext context,
        CancellationToken ct = default)
    {
        var accessToken = _tokenService.GenerateAccessToken(
            user, roles, context.MemberId, context.ClinicId, context.CountryCode);

        var rawRefreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id, null, ct);

        return new TokenResponseDto(accessToken, rawRefreshToken);
    }
}
