using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Email delivery queue with retry logic.
/// </summary>
public class EmailQueue : AuditableEntity
{
    public string ToEmail { get; set; } = null!;
    public string? ToName { get; set; }
    public string Subject { get; set; } = null!;
    public string Body { get; set; } = null!;
    public bool IsHtml { get; set; } = true;
    public EmailQueueStatus Status { get; set; } = EmailQueueStatus.Pending;
    public int Priority { get; set; } = 5;
    public int Attempts { get; set; } = 0;
    public int MaxAttempts { get; set; } = 3;
    public DateTimeOffset? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTimeOffset? ScheduledFor { get; set; }
}
