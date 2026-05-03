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

    public async Task<string> GenerateRefreshTokenAsync(
        Guid userId, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var now        = DateTimeOffset.UtcNow;
        var expiryTime = now.AddDays(_jwtOptions.RefreshTokenExpirationDays);

        var (entity, rawToken) = RefreshToken.Create(userId, expiryTime, ipAddress ?? _currentUserService.IpAddress);

        await _uow.RefreshTokens.AddAsync(entity, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Generated refresh token for user {UserId}, expires in {Days} days",
            userId, _jwtOptions.RefreshTokenExpirationDays);

        return rawToken;
    }

    public async Task<RefreshToken?> GetActiveRefreshTokenAsync(
        string rawToken, CancellationToken cancellationToken = default)
    {
        var hash = RefreshToken.Hash(rawToken);
        return await _uow.RefreshTokens.GetActiveTokenAsync(hash, DateTimeOffset.UtcNow, cancellationToken);
    }

    public async Task RevokeRefreshTokenAsync(
        string rawToken, string? ipAddress = null, string? replacedByRawToken = null,
        CancellationToken cancellationToken = default)
    {
        var hash         = RefreshToken.Hash(rawToken);
        var refreshToken = await _uow.RefreshTokens.GetActiveTokenAsync(hash, DateTimeOffset.UtcNow, cancellationToken);
        if (refreshToken is null) return;

        refreshToken.Revoke(ipAddress ?? _currentUserService.IpAddress, DateTimeOffset.UtcNow, replacedByRawToken);
        await _uow.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Revoked refresh token for user {UserId}", refreshToken.UserId);
    }

    public async Task RevokeAllUserRefreshTokensAsync(
        Guid userId, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        await _uow.RefreshTokens.RevokeAllForUserAsync(
            userId,
            ipAddress ?? _currentUserService.IpAddress,
            DateTimeOffset.UtcNow,
            cancellationToken);

        _logger.LogInformation("Revoked all refresh tokens for user {UserId}", userId);
    }

    public async Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var count = await _uow.RefreshTokens.DeleteExpiredAsync(DateTimeOffset.UtcNow, cancellationToken);
        if (count > 0)
            _logger.LogInformation("Cleaned up {Count} expired/revoked refresh tokens", count);
        return count;
    }
}
