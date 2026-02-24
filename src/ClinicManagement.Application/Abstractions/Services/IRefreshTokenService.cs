using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Services;

public interface IRefreshTokenService
{
    Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId, string? ipAddress, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetActiveRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(string token, string? ipAddress = null, string? replacedByToken = null, CancellationToken cancellationToken = default);
    Task RevokeAllUserRefreshTokensAsync(Guid userId, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
}
