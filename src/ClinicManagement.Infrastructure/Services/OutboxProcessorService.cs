using System.Text.Json;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities.Outbox;
using ClinicManagement.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Background service that processes outbox messages for reliable event publishing
/// Implements the Transactional Outbox Pattern
/// </summary>
public class OutboxProcessorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessorService> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(5);
    private readonly int _batchSize = 10;
    private readonly int _maxRetries = 3;

    public OutboxProcessorService(
        IServiceProvider serviceProvider,
        ILogger<OutboxProcessorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Processor Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(_processingInterval, stoppingToken);
        }

        _logger.LogInformation("Outbox Processor Service stopped");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

        // Get unprocessed messages
        var messages = await context.OutboxMessages
            .Where(m => m.ProcessedAt == null && m.RetryCount < _maxRetries)
            .OrderBy(m => m.OccurredAt)
            .Take(_batchSize)
            .ToListAsync(cancellationToken);

        if (!messages.Any())
        {
            return;
        }

        _logger.LogInformation("Processing {Count} outbox messages", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                // Deserialize and publish the domain event
                var domainEvent = DeserializeDomainEvent(message);
                
                if (domainEvent != null)
                {
                    await publisher.Publish(domainEvent, cancellationToken);
                    
                    // Mark as processed
                    message.MarkAsProcessed();
                    
                    _logger.LogInformation(
                        "Successfully processed outbox message {MessageId} of type {EventType}",
                        message.Id,
                        message.Type);
                }
                else
                {
                    message.RecordError($"Failed to deserialize event of type {message.Type}");
                    
                    _logger.LogWarning(
                        "Failed to deserialize outbox message {MessageId} of type {EventType}",
                        message.Id,
                        message.Type);
                }
            }
            catch (Exception ex)
            {
                message.RecordError(ex.Message);
                
                _logger.LogError(
                    ex,
                    "Error processing outbox message {MessageId} of type {EventType}. Retry count: {RetryCount}",
                    message.Id,
                    message.Type,
                    message.RetryCount);
            }
        }

        // Save all changes (processed status, errors, retry counts)
        await context.SaveChangesAsync(cancellationToken);
    }

    private DomainEvent? DeserializeDomainEvent(OutboxMessage message)
    {
        try
        {
            // Get the event type from the assembly
            var eventType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name == message.Type && typeof(DomainEvent).IsAssignableFrom(t));

            if (eventType == null)
            {
                _logger.LogWarning("Event type {EventType} not found in loaded assemblies", message.Type);
                return null;
            }

            // Deserialize the JSON content to the event type
            var domainEvent = JsonSerializer.Deserialize(message.Content, eventType) as DomainEvent;
            
            return domainEvent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing event of type {EventType}", message.Type);
            return null;
        }
    }
}
