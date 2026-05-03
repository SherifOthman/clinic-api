using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Services;

public interface IRefreshTokenService
{
    /// <summary>
    /// Generates a new refresh token, persists the hash, and returns the raw token
    /// string to send to the client.
    /// </summary>
    Task<string> GenerateRefreshTokenAsync(Guid userId, string? ipAddress = null, CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetActiveRefreshTokenAsync(string rawToken, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(string rawToken, string? ipAddress = null, string? replacedByRawToken = null, CancellationToken cancellationToken = default);
    Task RevokeAllUserRefreshTokensAsync(Guid userId, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
}
