using System;
using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities.Outbox;

/// <summary>
/// Represents a domain event stored in the outbox for reliable processing
/// Implements the Transactional Outbox Pattern
/// </summary>
public class OutboxMessage : BaseEntity
{
    // Private constructor for EF Core
    private OutboxMessage() { }

    /// <summary>
    /// The type name of the domain event (e.g., "AppointmentCreatedEvent")
    /// </summary>
    public string Type { get; private set; } = null!;

    /// <summary>
    /// The serialized JSON content of the domain event
    /// </summary>
    public string Content { get; private set; } = null!;

    /// <summary>
    /// When the domain event occurred
    /// </summary>
    public DateTime OccurredAt { get; private set; }

    /// <summary>
    /// When the event was successfully processed (null if not yet processed)
    /// </summary>
    public DateTime? ProcessedAt { get; private set; }

    /// <summary>
    /// Error message if processing failed
    /// </summary>
    public string? Error { get; private set; }

    /// <summary>
    /// Number of times processing has been attempted
    /// </summary>
    public int RetryCount { get; private set; }

    /// <summary>
    /// Factory method to create a new outbox message
    /// </summary>
    public static OutboxMessage Create(string type, string content, DateTime occurredAt)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Type cannot be empty", nameof(type));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty", nameof(content));

        return new OutboxMessage
        {
            Type = type,
            Content = content,
            OccurredAt = occurredAt,
            RetryCount = 0
        };
    }

    /// <summary>
    /// Marks the message as successfully processed
    /// </summary>
    public void MarkAsProcessed()
    {
        ProcessedAt = DateTime.UtcNow;
        Error = null;
    }

    /// <summary>
    /// Records a processing failure
    /// </summary>
    public void RecordError(string error)
    {
        Error = error;
        RetryCount++;
    }

    /// <summary>
    /// Checks if the message should be retried
    /// </summary>
    public bool ShouldRetry(int maxRetries = 3)
    {
        return ProcessedAt == null && RetryCount < maxRetries;
    }

    /// <summary>
    /// Checks if the message is pending processing
    /// </summary>
    public bool IsPending => ProcessedAt == null;

    /// <summary>
    /// Checks if the message has been processed
    /// </summary>
    public bool IsProcessed => ProcessedAt != null;

    /// <summary>
    /// Checks if the message has failed permanently (max retries exceeded)
    /// </summary>
    public bool HasFailed(int maxRetries = 3)
    {
        return ProcessedAt == null && RetryCount >= maxRetries;
    }
}
