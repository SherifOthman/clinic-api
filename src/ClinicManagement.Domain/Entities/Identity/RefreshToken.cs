using ClinicManagement.Domain.Common;
using System.Security.Cryptography;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// JWT refresh token.
/// System-generated — no human creator, so CreatedBy/UpdatedBy are meaningless.
/// Uses BaseEntity + explicit CreatedAt instead of AuditableEntity.
///
/// Security: Token stores the SHA-256 hash of the raw token value.
/// The raw token is only held in memory and sent to the client — never persisted.
/// A DB breach exposes only hashes, which cannot be reversed to valid tokens.
/// </summary>
public class RefreshToken : BaseEntity
{
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    /// <summary>SHA-256 hash (hex) of the raw token. Never the raw value.</summary>
    public string Token { get; private set; } = string.Empty;

    public Guid UserId { get; private set; }
    public DateTimeOffset ExpiryTime { get; private set; }
    public bool IsRevoked { get; private set; } = false;
    public string? CreatedByIp { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public string? RevokedByIp { get; private set; }

    /// <summary>SHA-256 hash of the token that replaced this one during rotation.</summary>
    public string? ReplacedByToken { get; private set; }

    public bool IsExpired(DateTimeOffset currentTime) => currentTime >= ExpiryTime;
    public bool IsActive(DateTimeOffset currentTime) => !IsRevoked && !IsExpired(currentTime);

    public void Revoke(string ipAddress, DateTimeOffset revokedAt, string? replacedByRawToken = null)
    {
        IsRevoked        = true;
        RevokedAt        = revokedAt;
        RevokedByIp      = ipAddress;
        ReplacedByToken  = replacedByRawToken is not null ? Hash(replacedByRawToken) : null;
    }

    /// <summary>
    /// Creates a new refresh token. Returns the entity (with hashed token stored)
    /// and the raw token string that must be sent to the client.
    /// </summary>
    public static (RefreshToken Entity, string RawToken) Create(
        Guid userId, DateTimeOffset expiryTime, string? ipAddress = null)
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);

        var rawToken = Convert.ToBase64String(bytes);

        var entity = new RefreshToken
        {
            Token       = Hash(rawToken),
            UserId      = userId,
            ExpiryTime  = expiryTime,
            CreatedByIp = ipAddress,
        };

        return (entity, rawToken);
    }

    /// <summary>SHA-256 hex hash — fast, collision-resistant, sufficient for random tokens.</summary>
    public static string Hash(string rawToken)
        => Convert.ToHexString(SHA256.HashData(Convert.FromBase64String(rawToken)));

    /// <summary>
    /// Temporarily overwrites Token with the raw value so the service layer can
    /// return it to the caller (who sends it to the client). Called immediately
    /// after persisting — the raw token is never written to the DB.
    /// </summary>
    public void SetRawTokenForTransport(string rawToken) => Token = rawToken;
}
