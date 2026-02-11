using ClinicManagement.API.Common;

namespace ClinicManagement.API.Entities;

/// <summary>
/// Refresh token for JWT authentication
/// Implements token rotation pattern: when refreshed, old token is revoked and replaced
/// Tracks IP addresses for security auditing
/// </summary>
public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public DateTime ExpiryTime { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public string? CreatedByIp { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    
    /// <summary>
    /// Token that replaced this one (for audit trail)
    /// </summary>
    public string? ReplacedByToken { get; set; }
    
    public User User { get; set; } = null!;
    
    public bool IsExpired(DateTime currentTime) => currentTime >= ExpiryTime;
    public bool IsActive(DateTime currentTime) => !IsRevoked && !IsExpired(currentTime);
}
