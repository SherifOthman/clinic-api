using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class RateLimitEntry : BaseEntity
{
    public string Identifier { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int RequestCount { get; set; }
    public DateTime WindowStart { get; set; }
    public DateTime ExpiresAt { get; set; }
}