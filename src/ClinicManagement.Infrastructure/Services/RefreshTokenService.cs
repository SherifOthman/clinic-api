using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.Options;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace ClinicManagement.Infrastructure.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<RefreshTokenService> _logger;

    public RefreshTokenService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IOptions<JwtOptions> jwtOptions,
        ILogger<RefreshTokenService> logger)
    {
        _unitOfWork = unitOfWork;
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
        var refreshToken = new RefreshToken
        {
            Token = token,
            UserId = userId,
            ExpiryTime = now.AddDays(_jwtOptions.RefreshTokenExpirationDays),
            CreatedAt = now,
            CreatedByIp = ipAddress ?? _currentUserService.IpAddress
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Generated refresh token for user {UserId} from IP {IpAddress}", 
            userId, refreshToken.CreatedByIp);
        return refreshToken;
    }

    public async Task<RefreshToken?> GetActiveRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.RefreshTokens.GetActiveTokenAsync(token, cancellationToken);
    }

    public async Task RevokeRefreshTokenAsync(string token, string? ipAddress = null, string? replacedByToken = null, CancellationToken cancellationToken = default)
    {
        var refreshToken = await _unitOfWork.RefreshTokens.GetActiveTokenAsync(token, cancellationToken);

        if (refreshToken != null)
        {
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = _dateTimeProvider.UtcNow;
            refreshToken.RevokedByIp = ipAddress ?? _currentUserService.IpAddress;
            refreshToken.ReplacedByToken = replacedByToken;

            _unitOfWork.RefreshTokens.Update(refreshToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Revoked refresh token for user {UserId} from IP {IpAddress}", 
                refreshToken.UserId, refreshToken.RevokedByIp);
        }
    }

    public async Task RevokeAllUserRefreshTokensAsync(Guid userId, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var activeTokens = await _unitOfWork.RefreshTokens.GetActiveTokensByUserIdAsync(userId, cancellationToken);
        var revokeIp = ipAddress ?? _currentUserService.IpAddress;
        var now = _dateTimeProvider.UtcNow;

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = now;
            token.RevokedByIp = revokeIp;
        }

        if (activeTokens.Any())
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Revoked {Count} refresh tokens for user {UserId}", activeTokens.Count, userId);
        }
    }

    public async Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var cleanedCount = await _unitOfWork.RefreshTokens.DeleteExpiredTokensAsync(cancellationToken);
        
        if (cleanedCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Cleaned up {Count} expired/revoked refresh tokens", cleanedCount);
        }

        return cleanedCount;
    }
}