using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Options;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace ClinicManagement.Infrastructure.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUserService;
    private readonly DateTimeProvider _dateTimeProvider;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<RefreshTokenService> _logger;

    public RefreshTokenService(
        ApplicationDbContext db,
        ICurrentUserService currentUserService,
        DateTimeProvider dateTimeProvider,
        IOptions<JwtOptions> jwtOptions,
        ILogger<RefreshTokenService> logger)
    {
        _db = db;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var token = Convert.ToBase64String(randomBytes);

        var now = _dateTimeProvider.UtcNow;
        
        // Use minutes if specified (for testing), otherwise use days (for production)
        var expiryTime = _jwtOptions.RefreshTokenExpirationMinutes.HasValue
            ? now.AddMinutes(_jwtOptions.RefreshTokenExpirationMinutes.Value)
            : now.AddDays(_jwtOptions.RefreshTokenExpirationDays);
        
        var refreshToken = new RefreshToken
        {
            Token = token,
            UserId = userId,
            ExpiryTime = expiryTime,
            CreatedAt = now,
            CreatedByIp = ipAddress ?? _currentUserService.IpAddress
        };

        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Generated refresh token for user {UserId} from IP {IpAddress}, expires in {ExpiryMinutes} minutes", 
            userId, refreshToken.CreatedByIp, (expiryTime - now).TotalMinutes);
        return refreshToken;
    }

    public async Task<RefreshToken?> GetActiveRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider.UtcNow;
        return await _db.RefreshTokens
            .Include(rt => rt.User) // Include User navigation property for token refresh
            .FirstOrDefaultAsync(rt => 
                rt.Token == token && 
                !rt.IsRevoked && 
                rt.ExpiryTime > now, 
                cancellationToken);
    }

    public async Task RevokeRefreshTokenAsync(string token, string? ipAddress = null, string? replacedByToken = null, CancellationToken cancellationToken = default)
    {
        var refreshToken = await GetActiveRefreshTokenAsync(token, cancellationToken);

        if (refreshToken != null)
        {
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = _dateTimeProvider.UtcNow;
            refreshToken.RevokedByIp = ipAddress ?? _currentUserService.IpAddress;
            refreshToken.ReplacedByToken = replacedByToken;

            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Revoked refresh token for user {UserId} from IP {IpAddress}", 
                refreshToken.UserId, refreshToken.RevokedByIp);
        }
    }

    public async Task RevokeAllUserRefreshTokensAsync(Guid userId, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider.UtcNow;
        var activeTokens = await _db.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiryTime > now)
            .ToListAsync(cancellationToken);
            
        var revokeIp = ipAddress ?? _currentUserService.IpAddress;

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = now;
            token.RevokedByIp = revokeIp;
        }

        if (activeTokens.Any())
        {
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Revoked {Count} refresh tokens for user {UserId}", activeTokens.Count, userId);
        }
    }

    public async Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider.UtcNow;
        var expiredTokens = await _db.RefreshTokens
            .Where(rt => rt.IsRevoked || rt.ExpiryTime <= now)
            .ToListAsync(cancellationToken);
        
        _db.RefreshTokens.RemoveRange(expiredTokens);
        
        if (expiredTokens.Any())
        {
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Cleaned up {Count} expired/revoked refresh tokens", expiredTokens.Count);
        }

        return expiredTokens.Count;
    }
}
