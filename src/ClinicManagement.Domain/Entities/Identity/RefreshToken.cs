using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

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
    public string? ReplacedByToken { get; set; }
    
    public User User { get; set; } = null!;
    
    public bool IsExpired(DateTime currentTime) => currentTime >= ExpiryTime;
    public bool IsActive(DateTime currentTime) => !IsRevoked && !IsExpired(currentTime);
}
