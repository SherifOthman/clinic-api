using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Persistence.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Jobs;

/// <summary>
/// Runs daily at 9am and notifies clinic owners when they are approaching
/// their subscription plan limits.
///
/// Reads today's already-aggregated metrics (written by UsageMetricsAggregationJob at 1am)
/// and compares them against the clinic's active plan limits.
///
/// Two thresholds:
///   80% — "Warning: you've used 80% of your monthly X limit"
///   95% — "Critical: you've almost reached your monthly X limit"
///
/// Deduplication: one notification per clinic per limit per threshold per month.
/// If a notification was already sent this month for the same limit + threshold, skip it.
/// </summary>
public class UsageLimitNotificationJob
{
    private const int WarnThresholdPercent     = 80;
    private const int CriticalThresholdPercent = 95;

    private readonly ApplicationDbContext _db;
    private readonly IEmailService _emailService;
    private readonly ILogger<UsageLimitNotificationJob> _logger;

    public UsageLimitNotificationJob(
        ApplicationDbContext db,
        IEmailService emailService,
        ILogger<UsageLimitNotificationJob> logger)
    {
        _db           = db;
        _emailService = emailService;
        _logger       = logger;
    }

    public async Task ExecuteAsync()
    {
        var now   = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(now.Date);

        // Load active clinics with their current subscription plan and today's metrics
        var clinics = await TenantGuard.AsSystemQuery(_db.Set<Clinic>())
            .Where(c => c.IsActive && !c.IsDeleted)
            .ToListAsync();

        _logger.LogInformation("Checking usage limits for {Count} clinics", clinics.Count);

        var notified = 0;

        foreach (var clinic in clinics)
        {
            try
            {
                // Get today's metrics (written by UsageMetricsAggregationJob at 1am)
                var metrics = await TenantGuard.AsSystemQuery(_db.Set<ClinicUsageMetrics>())
                    .FirstOrDefaultAsync(m => m.ClinicId == clinic.Id && m.MetricDate == today);

                if (metrics is null) continue; // aggregation hasn't run yet today

                // Get the active subscription and its plan limits
                var subscription = await TenantGuard.AsSystemQuery(_db.Set<ClinicSubscription>())
                    .Include(s => s.SubscriptionPlan)
                    .Where(s => s.ClinicId == clinic.Id &&
                                (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial))
                    .OrderByDescending(s => s.StartDate)
                    .FirstOrDefaultAsync();

                if (subscription?.SubscriptionPlan is null) continue;

                var plan  = subscription.SubscriptionPlan;
                var owner = await _db.Users.FirstOrDefaultAsync(u => u.Id == clinic.OwnerUserId);
                if (owner is null) continue;

                // Check each monthly limit and notify if threshold crossed
                var checks = new[]
                {
                    (Label: "patients",     Used: metrics.NewPatientsCount,   Max: plan.MaxPatientsPerMonth),
                    (Label: "appointments", Used: metrics.AppointmentsCount,  Max: plan.MaxAppointmentsPerMonth),
                    (Label: "invoices",     Used: metrics.InvoicesCount,      Max: plan.MaxInvoicesPerMonth),
                };

                foreach (var (label, used, max) in checks)
                {
                    if (max <= 0) continue; // unlimited (-1) or not configured

                    var percent = (int)((double)used / max * 100);

                    if (percent >= CriticalThresholdPercent)
                        await NotifyIfNotAlreadySentAsync(clinic, owner, label, used, max, CriticalThresholdPercent, now);
                    else if (percent >= WarnThresholdPercent)
                        await NotifyIfNotAlreadySentAsync(clinic, owner, label, used, max, WarnThresholdPercent, now);
                }

                notified++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking limits for clinic {ClinicId}", clinic.Id);
            }
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Usage limit check complete — processed {Count} clinics", notified);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task NotifyIfNotAlreadySentAsync(
        Clinic clinic, User owner,
        string limitLabel, int used, int max, int thresholdPercent,
        DateTimeOffset now)
    {
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);

        // Dedup: skip if we already sent this exact notification this month
        var alreadySent = await _db.Set<Notification>()
            .AnyAsync(n =>
                n.UserId == owner.Id &&
                n.CreatedAt >= monthStart &&
                n.Title.Contains(limitLabel) &&
                n.Title.Contains(thresholdPercent.ToString()));

        if (alreadySent) return;

        var isCritical = thresholdPercent >= CriticalThresholdPercent;
        var percent     = (int)((double)used / max * 100);

        var title   = isCritical
            ? $"Critical: {percent}% of monthly {limitLabel} limit used"
            : $"Warning: {percent}% of monthly {limitLabel} limit used";

        var message = $"You have used {used} of {max} {limitLabel} this month ({percent}%). " +
                      (isCritical
                          ? "You are about to reach your plan limit. Consider upgrading your plan."
                          : "You are approaching your plan limit.");

        // In-app notification
        _db.Set<Notification>().Add(new Notification
        {
            UserId    = owner.Id,
            Type      = isCritical ? NotificationType.Error : NotificationType.Warning,
            Title     = title,
            Message   = message,
            ActionUrl = "/settings/subscription",
            ExpiresAt = now.AddDays(7),
        });

        // Email via queue (processed by EmailQueueProcessorJob)
        _db.Set<EmailQueue>().Add(new EmailQueue
        {
            ToEmail  = owner.Email!,
            ToName   = owner.FullName,
            Subject  = $"[{clinic.Name}] {title}",
            Body     = BuildEmailBody(owner.FullName, clinic.Name, limitLabel, used, max, percent, isCritical),
            IsHtml   = true,
            Priority = isCritical ? 1 : 2,
        });

        _logger.LogInformation(
            "Limit notification queued for clinic {ClinicId} — {Label} at {Percent}% ({Threshold}% threshold)",
            clinic.Id, limitLabel, percent, thresholdPercent);
    }

    private static string BuildEmailBody(
        string ownerName, string clinicName,
        string limitLabel, int used, int max, int percent, bool isCritical)
    {
        var color      = isCritical ? "#dc2626" : "#f59e0b";
        var headerText = isCritical ? "⚠️ Usage Limit Critical" : "📊 Usage Limit Warning";
        var callToAction = isCritical
            ? "You are about to reach your plan limit. Upgrade your plan to avoid service interruption."
            : "You are approaching your plan limit. Consider upgrading before you reach the limit.";

        return $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: {color}; padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0;'>{headerText}</h1>
    </div>
    <div style='background: #fff; padding: 30px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 10px 10px;'>
        <p>Hello {ownerName},</p>
        <p>This is a usage alert for <strong>{clinicName}</strong>.</p>
        <div style='background: #f9fafb; border-left: 4px solid {color}; padding: 16px; border-radius: 4px; margin: 20px 0;'>
            <p style='margin: 0; font-size: 18px; font-weight: bold;'>{limitLabel.ToUpperInvariant()}</p>
            <p style='margin: 8px 0 0 0;'>Used: <strong>{used} / {max}</strong> ({percent}% of monthly limit)</p>
        </div>
        <p>{callToAction}</p>
        <div style='text-align: center; margin: 30px 0;'>
            <a href='/settings/subscription'
               style='background: {color}; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                View Subscription
            </a>
        </div>
    </div>
    <div style='text-align: center; padding: 20px; color: #6b7280; font-size: 12px;'>
        <p>© 2025 Clinic Management System. All rights reserved.</p>
    </div>
</body>
</html>";
    }
}
