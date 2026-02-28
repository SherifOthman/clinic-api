using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

public class SubscriptionExpiryNotificationJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SubscriptionExpiryNotificationJob> _logger;

    public SubscriptionExpiryNotificationJob(
        IServiceProvider serviceProvider,
        ILogger<SubscriptionExpiryNotificationJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Subscription Expiry Notification Job started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                var nextRun = GetNextRunTime(now);
                var delay = nextRun - now;

                _logger.LogInformation("Next subscription expiry notification check scheduled for {NextRun} UTC", nextRun);
                
                await Task.Delay(delay, stoppingToken);
                
                if (!stoppingToken.IsCancellationRequested)
                {
                    await ProcessExpiringSubscriptionsAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Subscription Expiry Notification Job is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during subscription expiry notification processing");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private static DateTime GetNextRunTime(DateTime now)
    {
        // Schedule to run daily at 9:00 AM UTC
        var nextRun = new DateTime(now.Year, now.Month, now.Day, 9, 0, 0, DateTimeKind.Utc);
        
        if (now >= nextRun)
        {
            nextRun = nextRun.AddDays(1);
        }
        
        return nextRun;
    }

    private async Task ProcessExpiringSubscriptionsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        try
        {
            var expiryThreshold = DateTime.UtcNow.AddDays(7);
            var expiringSubscriptions = await context.ClinicSubscriptions
                .Where(s => s.EndDate.HasValue && 
                           s.EndDate.Value <= expiryThreshold && 
                           s.EndDate.Value > DateTime.UtcNow &&
                           s.Status == SubscriptionStatus.Active)
                .ToListAsync(cancellationToken);

            if (!expiringSubscriptions.Any())
            {
                _logger.LogDebug("No expiring subscriptions found");
                return;
            }

            _logger.LogInformation("Processing {Count} expiring subscriptions", expiringSubscriptions.Count);

            var notificationCount = 0;
            var errorCount = 0;

            foreach (var subscription in expiringSubscriptions)
            {
                try
                {
                    await CreateNotificationAndEmailAsync(context, subscription, cancellationToken);
                    notificationCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, 
                        "Error creating notification for subscription {SubscriptionId} of clinic {ClinicId}", 
                        subscription.Id, subscription.ClinicId);
                    errorCount++;
                }
            }

            _logger.LogInformation(
                "Subscription expiry notification processing completed: {NotificationCount} notifications created, {ErrorCount} errors", 
                notificationCount, errorCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error during subscription expiry notification processing");
            throw;
        }
    }

    private async Task CreateNotificationAndEmailAsync(
        IApplicationDbContext context,
        ClinicSubscription subscription,
        CancellationToken cancellationToken)
    {
        var clinic = await context.Clinics
            .FirstOrDefaultAsync(c => c.Id == subscription.ClinicId, cancellationToken);
        
        if (clinic == null)
        {
            _logger.LogWarning("Clinic {ClinicId} not found for subscription {SubscriptionId}", 
                subscription.ClinicId, subscription.Id);
            return;
        }

        var owner = await context.Users
            .FirstOrDefaultAsync(u => u.Id == clinic.OwnerUserId, cancellationToken);
        
        if (owner == null)
        {
            _logger.LogWarning("Owner user {UserId} not found for clinic {ClinicId}", 
                clinic.OwnerUserId, clinic.Id);
            return;
        }

        var daysUntilExpiry = subscription.EndDate.HasValue 
            ? (subscription.EndDate.Value.Date - DateTime.UtcNow.Date).Days 
            : 0;

        var notification = new Notification
        {
            UserId = owner.Id,
            Type = NotificationType.Warning,
            Title = "Subscription Expiring Soon",
            Message = $"Your subscription for {clinic.Name} will expire on {subscription.EndDate:yyyy-MM-dd} ({daysUntilExpiry} days remaining). Please renew to avoid service interruption.",
            ActionUrl = "/billing/renew"
        };
        context.Notifications.Add(notification);

        var emailAddress = !string.IsNullOrWhiteSpace(clinic.BillingEmail) 
            ? clinic.BillingEmail 
            : owner.Email;

        var emailBody = BuildEmailBody(clinic.Name, subscription.EndDate, daysUntilExpiry);

        var email = new EmailQueue
        {
            ToEmail = emailAddress!,
            ToName = $"{owner.FirstName} {owner.LastName}",
            Subject = "Subscription Expiring Soon - Action Required",
            Body = emailBody,
            IsHtml = true,
            Priority = 3
        };
        context.EmailQueue.Add(email);

        await context.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Created notification and queued email for clinic {ClinicId} (expires in {Days} days)", 
            clinic.Id, daysUntilExpiry);
    }

    private static string BuildEmailBody(string clinicName, DateTime? endDate, int daysUntilExpiry)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #f8d7da; color: #721c24; padding: 15px; border-radius: 5px; margin-bottom: 20px; }}
        .content {{ padding: 20px; background-color: #f9f9f9; border-radius: 5px; }}
        .footer {{ margin-top: 20px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #666; }}
        .button {{ display: inline-block; padding: 10px 20px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px; margin-top: 15px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h2>⚠️ Subscription Expiring Soon</h2>
        </div>
        <div class=""content"">
            <p>Dear Clinic Owner,</p>
            <p>This is a reminder that your subscription for <strong>{clinicName}</strong> is expiring soon.</p>
            <p><strong>Expiration Date:</strong> {endDate:MMMM dd, yyyy}</p>
            <p><strong>Days Remaining:</strong> {daysUntilExpiry} days</p>
            <p>To ensure uninterrupted access to your clinic management system, please renew your subscription before the expiration date.</p>
            <a href=""#"" class=""button"">Renew Subscription</a>
        </div>
        <div class=""footer"">
            <p>This is an automated notification from your Clinic Management System.</p>
            <p>If you have any questions, please contact our support team.</p>
        </div>
    </div>
</body>
</html>";
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Subscription Expiry Notification Job is stopping");
        await base.StopAsync(stoppingToken);
    }
}
