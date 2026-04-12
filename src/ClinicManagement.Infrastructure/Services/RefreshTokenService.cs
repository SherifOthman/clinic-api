using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Infrastructure.Options;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<RefreshToken>  _refreshTokens;
    private readonly ICurrentUserService  _currentUserService;
    private readonly DateTimeProvider     _dateTimeProvider;
    private readonly JwtOptions           _jwtOptions;
    private readonly ILogger<RefreshTokenService> _logger;

    public RefreshTokenService(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        DateTimeProvider dateTimeProvider,
        IOptions<JwtOptions> jwtOptions,
        ILogger<RefreshTokenService> logger)
    {
        _context            = context;
        _refreshTokens      = context.Set<RefreshToken>();
        _currentUserService = currentUserService;
        _dateTimeProvider   = dateTimeProvider;
        _jwtOptions         = jwtOptions.Value;
        _logger             = logger;
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider.UtcNow;
        var expiryTime = _jwtOptions.RefreshTokenExpirationMinutes.HasValue
            ? now.AddMinutes(_jwtOptions.RefreshTokenExpirationMinutes.Value)
            : now.AddDays(_jwtOptions.RefreshTokenExpirationDays);

        var refreshToken = RefreshToken.Create(userId, expiryTime, ipAddress ?? _currentUserService.IpAddress);

        _refreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Generated refresh token for user {UserId}, expires in {Minutes} minutes",
            userId, (expiryTime - now).TotalMinutes);
        return refreshToken;
    }

    public async Task<RefreshToken?> GetActiveRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var now          = _dateTimeProvider.UtcNow;
        var refreshToken = await _refreshTokens.FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
        return refreshToken is { IsRevoked: false } && refreshToken.ExpiryTime > now ? refreshToken : null;
    }

    public async Task RevokeRefreshTokenAsync(string token, string? ipAddress = null, string? replacedByToken = null, CancellationToken cancellationToken = default)
    {
        var refreshToken = await GetActiveRefreshTokenAsync(token, cancellationToken);
        if (refreshToken is null) return;

        refreshToken.Revoke(ipAddress ?? _currentUserService.IpAddress, _dateTimeProvider.UtcNow, replacedByToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Revoked refresh token for user {UserId}", refreshToken.UserId);
    }

    public async Task RevokeAllUserRefreshTokensAsync(Guid userId, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var now      = _dateTimeProvider.UtcNow;
        var revokeIp = ipAddress ?? _currentUserService.IpAddress;

        var count = await _refreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiryTime > now)
            .ExecuteUpdateAsync(s => s
                .SetProperty(rt => rt.IsRevoked, true)
                .SetProperty(rt => rt.RevokedAt, now)
                .SetProperty(rt => rt.RevokedByIp, revokeIp),
                cancellationToken);

        if (count > 0)
            _logger.LogInformation("Revoked {Count} refresh tokens for user {UserId}", count, userId);
    }

    public async Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var now   = _dateTimeProvider.UtcNow;
        var count = await _refreshTokens
            .Where(rt => rt.IsRevoked || rt.ExpiryTime <= now)
            .ExecuteDeleteAsync(cancellationToken);

        if (count > 0)
            _logger.LogInformation("Cleaned up {Count} expired/revoked refresh tokens", count);
        return count;
    }
}
