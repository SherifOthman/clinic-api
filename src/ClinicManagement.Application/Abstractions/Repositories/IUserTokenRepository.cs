using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Repositories;

/// <summary>
/// Repository for OTP tokens used in email confirmation and password reset.
/// </summary>
public interface IUserTokenRepository
{
    Task AddAsync(UserToken token, CancellationToken ct = default);

    /// <summary>
    /// Finds an active (not used, not expired) token by its hash.
    /// </summary>
    Task<UserToken?> GetActiveByHashAsync(string tokenHash, DateTimeOffset now, CancellationToken ct = default);

    /// <summary>
    /// Invalidates all existing tokens for a user+type before issuing a new one.
    /// Prevents accumulation of stale OTPs.
    /// </summary>
    Task InvalidateAllAsync(Guid userId, string tokenType, CancellationToken ct = default);

    /// <summary>
    /// Deletes expired and used tokens. Called by the cleanup background job.
    /// </summary>
    Task<int> DeleteExpiredAsync(DateTimeOffset now, CancellationToken ct = default);
}
