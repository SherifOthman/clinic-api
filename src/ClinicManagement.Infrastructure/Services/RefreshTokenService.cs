using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Common.Options;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly DateTimeProvider _dateTimeProvider;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<RefreshTokenService> _logger;

    public RefreshTokenService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        DateTimeProvider dateTimeProvider,
        IOptions<JwtOptions> jwtOptions,
        ILogger<RefreshTokenService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider.UtcNow;
        
        var expiryTime = _jwtOptions.RefreshTokenExpirationMinutes.HasValue
            ? now.AddMinutes(_jwtOptions.RefreshTokenExpirationMinutes.Value)
            : now.AddDays(_jwtOptions.RefreshTokenExpirationDays);
        
        var refreshToken = RefreshToken.Create(
            userId,
            expiryTime,
            ipAddress ?? _currentUserService.IpAddress
        );

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Generated refresh token for user {UserId} from IP {IpAddress}, expires in {ExpiryMinutes} minutes", 
            userId, refreshToken.CreatedByIp, (expiryTime - now).TotalMinutes);
        return refreshToken;
    }

    public async Task<RefreshToken?> GetActiveRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider.UtcNow;
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
        
        if (refreshToken != null && !refreshToken.IsRevoked && refreshToken.ExpiryTime > now)
        {
            return refreshToken;
        }
        
        return null;
    }

    public async Task RevokeRefreshTokenAsync(string token, string? ipAddress = null, string? replacedByToken = null, CancellationToken cancellationToken = default)
    {
        var refreshToken = await GetActiveRefreshTokenAsync(token, cancellationToken);

        if (refreshToken != null)
        {
            refreshToken.Revoke(
                ipAddress ?? _currentUserService.IpAddress,
                _dateTimeProvider.UtcNow,
                replacedByToken
            );

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Revoked refresh token for user {UserId} from IP {IpAddress}", 
                refreshToken.UserId, refreshToken.RevokedByIp);
        }
    }

    public async Task RevokeAllUserRefreshTokensAsync(Guid userId, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider.UtcNow;
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiryTime > now)
            .ToListAsync(cancellationToken);
            
        var revokeIp = ipAddress ?? _currentUserService.IpAddress;

        foreach (var token in activeTokens)
        {
            token.Revoke(revokeIp, now);
        }

        if (activeTokens.Any())
        {
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Revoked {Count} refresh tokens for user {UserId}", activeTokens.Count, userId);
        }
    }

    public async Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider.UtcNow;
        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.IsRevoked || rt.ExpiryTime <= now)
            .ToListAsync(cancellationToken);

        if (expiredTokens.Any())
        {
            _context.RefreshTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Cleaned up {Count} expired/revoked refresh tokens", expiredTokens.Count);
            return expiredTokens.Count;
        }

        return 0;
    }
}

