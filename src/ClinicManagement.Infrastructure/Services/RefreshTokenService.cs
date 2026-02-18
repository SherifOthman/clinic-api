using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Options;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace ClinicManagement.Infrastructure.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly DateTimeProvider _dateTimeProvider;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<RefreshTokenService> _logger;

    public RefreshTokenService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        DateTimeProvider dateTimeProvider,
        IOptions<JwtOptions> jwtOptions,
        ILogger<RefreshTokenService> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(int userId, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var token = Convert.ToBase64String(randomBytes);

        var now = _dateTimeProvider.UtcNow;
        
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

        await _unitOfWork.RefreshTokens.AddAsync(refreshToken, cancellationToken);

        _logger.LogInformation("Generated refresh token for user {UserId} from IP {IpAddress}, expires in {ExpiryMinutes} minutes", 
            userId, refreshToken.CreatedByIp, (expiryTime - now).TotalMinutes);
        return refreshToken;
    }

    public async Task<RefreshToken?> GetActiveRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider.UtcNow;
        var refreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(token, cancellationToken);
        
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
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = _dateTimeProvider.UtcNow;
            refreshToken.RevokedByIp = ipAddress ?? _currentUserService.IpAddress;
            refreshToken.ReplacedByToken = replacedByToken;

            await _unitOfWork.RefreshTokens.UpdateAsync(refreshToken, cancellationToken);

            _logger.LogInformation("Revoked refresh token for user {UserId} from IP {IpAddress}", 
                refreshToken.UserId, refreshToken.RevokedByIp);
        }
    }

    public async Task RevokeAllUserRefreshTokensAsync(int userId, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider.UtcNow;
        var activeTokens = await _unitOfWork.RefreshTokens.GetActiveTokensByUserIdAsync(userId, cancellationToken);
            
        var revokeIp = ipAddress ?? _currentUserService.IpAddress;

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = now;
            token.RevokedByIp = revokeIp;
            await _unitOfWork.RefreshTokens.UpdateAsync(token, cancellationToken);
        }

        if (activeTokens.Any())
        {
            var count = activeTokens.Count();
            _logger.LogInformation("Revoked {Count} refresh tokens for user {UserId}", count, userId);
        }
    }

    public async Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var count = await _unitOfWork.RefreshTokens.DeleteExpiredTokensAsync(cancellationToken);
        
        if (count > 0)
        {
            _logger.LogInformation("Cleaned up {Count} expired/revoked refresh tokens", count);
        }

        return count;
    }
}
