using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Email delivery queue with retry logic.
/// Processes emails asynchronously with priority and scheduling support.
/// </summary>
public class EmailQueue : BaseEntity
{
    public string ToEmail { get; set; } = null!;
    public string? ToName { get; set; }
    public string Subject { get; set; } = null!;
    public string Body { get; set; } = null!;
    public bool IsHtml { get; set; } = true;
    public EmailQueueStatus Status { get; set; } = EmailQueueStatus.Pending;
    public int Priority { get; set; } = 5; // 1=Highest, 10=Lowest
    public int Attempts { get; set; } = 0;
    public int MaxAttempts { get; set; } = 3;
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ScheduledFor { get; set; }
}
