using ClinicManagement.Domain.Common;
using System.Security.Cryptography;

namespace ClinicManagement.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public string Token { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public DateTime ExpiryTime { get; private set; }
    public bool IsRevoked { get; private set; } = false;
    public DateTime CreatedAt { get; private set; }
    public string? CreatedByIp { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? RevokedByIp { get; private set; }
    public string? ReplacedByToken { get; private set; }
    
    public bool IsExpired(DateTime currentTime) => currentTime >= ExpiryTime;
    public bool IsActive(DateTime currentTime) => !IsRevoked && !IsExpired(currentTime);

    public void Revoke(string ipAddress, DateTime revokedAt, string? replacedByToken = null)
    {
        IsRevoked = true;
        RevokedAt = revokedAt;
        RevokedByIp = ipAddress;
        ReplacedByToken = replacedByToken;
    }

    public static RefreshToken Create(
        Guid userId,
        DateTime expiryTime,
        string? ipAddress = null)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var token = Convert.ToBase64String(randomBytes);

        return new RefreshToken
        {
            Token = token,
            UserId = userId,
            ExpiryTime = expiryTime,
            CreatedByIp = ipAddress
        };
    }
}
