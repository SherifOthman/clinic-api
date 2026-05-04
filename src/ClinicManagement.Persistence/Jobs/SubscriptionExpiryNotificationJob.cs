using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Persistence.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Jobs;

public class SubscriptionExpiryNotificationJob
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<SubscriptionExpiryNotificationJob> _logger;

    public SubscriptionExpiryNotificationJob(ApplicationDbContext db, ILogger<SubscriptionExpiryNotificationJob> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        var now             = DateTimeOffset.UtcNow;
        var expiryThreshold = now.AddDays(7);

        var expiring = await TenantGuard.AsSystemQuery(_db.Set<ClinicSubscription>())
            .Where(s =>
                s.Status == SubscriptionStatus.Active &&
                s.EndDate > now &&
                s.EndDate <= expiryThreshold)
            .ToListAsync();

        if (!expiring.Any()) return;

        _logger.LogInformation("Processing {Count} expiring subscriptions", expiring.Count);

        foreach (var subscription in expiring)
        {
            try
            {
                var clinic = await TenantGuard.AsSystemQuery(_db.Set<Clinic>())
                    .FirstOrDefaultAsync(c => c.Id == subscription.ClinicId);
                if (clinic is null) continue;

                var owner = await _db.Users
                    .FirstOrDefaultAsync(u => u.Id == clinic.OwnerUserId);
                if (owner is null) continue;

                var daysLeft = (int)(subscription.EndDate!.Value.Date - now.Date).TotalDays;

                _db.Set<Notification>().Add(new Notification
                {
                    UserId    = owner.Id,
                    Type      = NotificationType.Warning,
                    Title     = "Subscription Expiring Soon",
                    Message   = $"Your subscription for {clinic.Name} expires on {subscription.EndDate:yyyy-MM-dd} ({daysLeft} days remaining).",
                    ActionUrl = "/billing/renew",
                });

                _db.Set<EmailQueue>().Add(new EmailQueue
                {
                    ToEmail  = !string.IsNullOrWhiteSpace(clinic.BillingEmail) ? clinic.BillingEmail : owner.Email!,
                    ToName   = owner.FullName,
                    Subject  = "Subscription Expiring Soon - Action Required",
                    Body     = $"<p>Your subscription for <strong>{clinic.Name}</strong> expires in {daysLeft} days ({subscription.EndDate:yyyy-MM-dd}). Please renew to avoid interruption.</p>",
                    IsHtml   = true,
                    Priority = 3,
                });

                _logger.LogDebug("Notification queued for clinic {ClinicId} (expires in {Days} days)", clinic.Id, daysLeft);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing subscription {SubscriptionId}", subscription.Id);
            }
        }

        await _db.SaveChangesAsync();
    }
}
