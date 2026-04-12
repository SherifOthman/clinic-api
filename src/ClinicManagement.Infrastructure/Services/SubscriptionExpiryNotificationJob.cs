using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

public class SubscriptionExpiryNotificationJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SubscriptionExpiryNotificationJob> _logger;

    public SubscriptionExpiryNotificationJob(IServiceProvider serviceProvider, ILogger<SubscriptionExpiryNotificationJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger          = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Subscription Expiry Notification Job started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now     = DateTimeOffset.UtcNow;
                var nextRun = GetNextRunTime(now);
                _logger.LogInformation("Next subscription expiry check scheduled for {NextRun} UTC", nextRun);
                await Task.Delay(nextRun - now, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                    await ProcessExpiringSubscriptionsAsync(stoppingToken);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during subscription expiry notification processing");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private static DateTimeOffset GetNextRunTime(DateTimeOffset now)
    {
        var next = new DateTimeOffset(now.Year, now.Month, now.Day, 9, 0, 0, TimeSpan.Zero);
        return now >= next ? next.AddDays(1) : next;
    }

    private async Task ProcessExpiringSubscriptionsAsync(CancellationToken cancellationToken)
    {
        using var scope         = _serviceProvider.CreateScope();
        var context             = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var subscriptions       = context.Set<ClinicSubscription>();
        var clinics             = context.Set<Clinic>();
        var users               = context.Set<User>();
        var notifications       = context.Set<Notification>();
        var emailQueue          = context.Set<EmailQueue>();

        var expiryThreshold = DateTimeOffset.UtcNow.AddDays(7);
        var expiring = await subscriptions
            .Where(s => s.EndDate.HasValue &&
                        s.EndDate.Value <= expiryThreshold &&
                        s.EndDate.Value > DateTimeOffset.UtcNow &&
                        s.Status == SubscriptionStatus.Active)
            .ToListAsync(cancellationToken);

        if (!expiring.Any()) return;

        _logger.LogInformation("Processing {Count} expiring subscriptions", expiring.Count);

        foreach (var subscription in expiring)
        {
            try
            {
                var clinic = await clinics.FirstOrDefaultAsync(c => c.Id == subscription.ClinicId, cancellationToken);
                if (clinic is null) continue;

                var owner = await users.FirstOrDefaultAsync(u => u.Id == clinic.OwnerUserId, cancellationToken);
                if (owner is null) continue;

                var daysLeft = subscription.EndDate.HasValue
                    ? (subscription.EndDate.Value.Date - DateTimeOffset.UtcNow.Date).Days
                    : 0;

                notifications.Add(new Notification
                {
                    UserId    = owner.Id,
                    Type      = NotificationType.Warning,
                    Title     = "Subscription Expiring Soon",
                    Message   = $"Your subscription for {clinic.Name} expires on {subscription.EndDate:yyyy-MM-dd} ({daysLeft} days remaining).",
                    ActionUrl = "/billing/renew",
                });

                emailQueue.Add(new EmailQueue
                {
                    ToEmail  = !string.IsNullOrWhiteSpace(clinic.BillingEmail) ? clinic.BillingEmail : owner.Email!,
                    ToName   = $"{owner.FirstName} {owner.LastName}",
                    Subject  = "Subscription Expiring Soon - Action Required",
                    Body     = $"<p>Your subscription for <strong>{clinic.Name}</strong> expires in {daysLeft} days ({subscription.EndDate:yyyy-MM-dd}). Please renew to avoid interruption.</p>",
                    IsHtml   = true,
                    Priority = 3,
                });

                await context.SaveChangesAsync(cancellationToken);
                _logger.LogDebug("Notification queued for clinic {ClinicId} (expires in {Days} days)", clinic.Id, daysLeft);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing subscription {SubscriptionId}", subscription.Id);
            }
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Subscription Expiry Notification Job stopping");
        await base.StopAsync(stoppingToken);
    }
}
