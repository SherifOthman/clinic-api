using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<RefreshTokenService> _logger;

    public RefreshTokenService(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        IOptions<JwtOptions> jwtOptions,
        ILogger<RefreshTokenService> logger)
    {
        _uow                = uow;
        _currentUserService = currentUserService;
        _jwtOptions         = jwtOptions.Value;
        _logger             = logger;
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var expiryTime = _jwtOptions.RefreshTokenExpirationMinutes.HasValue
            ? now.AddMinutes(_jwtOptions.RefreshTokenExpirationMinutes.Value)
            : now.AddDays(_jwtOptions.RefreshTokenExpirationDays);

        var refreshToken = RefreshToken.Create(userId, expiryTime, ipAddress ?? _currentUserService.IpAddress);

        await _uow.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Generated refresh token for user {UserId}, expires in {Minutes} minutes",
            userId, (expiryTime - now).TotalMinutes);
        return refreshToken;
    }

    public async Task<RefreshToken?> GetActiveRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _uow.RefreshTokens.GetActiveTokenAsync(token, DateTimeOffset.UtcNow, cancellationToken);
    }

    public async Task RevokeRefreshTokenAsync(string token, string? ipAddress = null, string? replacedByToken = null, CancellationToken cancellationToken = default)
    {
        var refreshToken = await GetActiveRefreshTokenAsync(token, cancellationToken);
        if (refreshToken is null) return;

        refreshToken.Revoke(ipAddress ?? _currentUserService.IpAddress, DateTimeOffset.UtcNow, replacedByToken);
        await _uow.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Revoked refresh token for user {UserId}", refreshToken.UserId);
    }

    public async Task RevokeAllUserRefreshTokensAsync(Guid userId, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        await _uow.RefreshTokens.RevokeAllForUserAsync(
            userId,
            ipAddress ?? _currentUserService.IpAddress,
            DateTimeOffset.UtcNow,
            cancellationToken);

        _logger.LogInformation("Revoked refresh tokens for user {UserId}", userId);
    }

    public async Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var count = await _uow.RefreshTokens.DeleteExpiredAsync(DateTimeOffset.UtcNow, cancellationToken);
        if (count > 0)
            _logger.LogInformation("Cleaned up {Count} expired/revoked refresh tokens", count);
        return count;
    }
}
