using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace ClinicManagement.Application.Features.Auth.Commands;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<TokenResponseDto>>
{
    // Per-user lock prevents concurrent refresh races (token rotation).
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _userLocks = new();

    private readonly IUnitOfWork _uow;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ITokenIssuer _tokenIssuer;
    private readonly ILogger<RefreshTokenHandler> _logger;

    public RefreshTokenHandler(
        IUnitOfWork uow,
        IRefreshTokenService refreshTokenService,
        ITokenIssuer tokenIssuer,
        ILogger<RefreshTokenHandler> logger)
    {
        _uow                 = uow;
        _refreshTokenService = refreshTokenService;
        _tokenIssuer         = tokenIssuer;
        _logger              = logger;
    }

    public async Task<Result<TokenResponseDto>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var tokenEntity = await _refreshTokenService.GetActiveRefreshTokenAsync(request.Token, ct);
        if (tokenEntity is null)
        {
            _logger.LogWarning("Invalid or expired refresh token");
            return Result.Failure<TokenResponseDto>(ErrorCodes.TOKEN_INVALID, "Invalid or expired refresh token");
        }

        var userId    = tokenEntity.UserId;
        var semaphore = _userLocks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync(ct);
        try
        {
            var user = await _uow.Users.GetByIdWithPersonAsync(userId, ct);
            if (user is null)
            {
                _logger.LogWarning("User not found for refresh token: {UserId}", userId);
                return Result.Failure<TokenResponseDto>(ErrorCodes.USER_NOT_FOUND, "User not found");
            }

            var roles = (await _uow.Users.GetRolesByUserIdAsync(user.Id, ct))
                .Select(r => r.RoleName)
                .ToList();

            var contextResult = await _tokenIssuer.ResolveContextAsync(user.Id, roles, ct);
            if (contextResult.IsFailure)
                return Result.Failure<TokenResponseDto>(contextResult.ErrorCode!, contextResult.ErrorMessage!);

            // Revoke old token before issuing new one (rotation)
            var newTokens = await _tokenIssuer.IssueTokenPairAsync(user, roles, contextResult.Value!, ct);
            await _refreshTokenService.RevokeRefreshTokenAsync(request.Token, null, newTokens.RefreshToken, ct);

            _logger.LogInformation("Token refreshed for user {UserId} ({ClientType})",
                userId, request.IsMobile ? "mobile" : "web");

            return Result.Success(newTokens);
        }
        finally
        {
            semaphore.Release();
            if (semaphore.CurrentCount == 1)
                _userLocks.TryRemove(userId, out _);
        }
    }
}
