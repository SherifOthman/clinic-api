using ClinicManagement.Application.Common.EmailTemplates;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Persistence.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Jobs;

/// <summary>
/// Runs daily at 9am. Reads today's aggregated metrics (written by UsageMetricsAggregationJob
/// at 1am) and notifies clinic owners when they approach their plan limits.
///
/// Thresholds:  80% → Warning   |   95% → Critical
/// Dedup:       one notification per clinic × limit × threshold per calendar month.
/// </summary>
public class UsageLimitNotificationJob
{
    // ── Thresholds ────────────────────────────────────────────────────────────

    private const int WarnThresholdPercent     = 80;
    private const int CriticalThresholdPercent = 95;

    // ── Dependencies ──────────────────────────────────────────────────────────

    private readonly ApplicationDbContext _db;
    private readonly ILogger<UsageLimitNotificationJob> _logger;

    public UsageLimitNotificationJob(
        ApplicationDbContext db,
        ILogger<UsageLimitNotificationJob> logger)
    {
        _db     = db;
        _logger = logger;
    }

    // ── Entry point ───────────────────────────────────────────────────────────

    public async Task ExecuteAsync()
    {
        var now    = DateTimeOffset.UtcNow;
        var today  = DateOnly.FromDateTime(now.Date);

        var clinics = await LoadActiveClinicsAsync();
        _logger.LogInformation("Checking usage limits for {Count} clinics", clinics.Count);

        var processed = 0;

        foreach (var clinic in clinics)
        {
            try
            {
                await ProcessClinicAsync(clinic, today, now);
                processed++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking limits for clinic {ClinicId}", clinic.Id);
            }
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Usage limit check complete — {Count}/{Total} clinics processed", processed, clinics.Count);
    }

    // ── Per-clinic logic ──────────────────────────────────────────────────────

    private async Task ProcessClinicAsync(Clinic clinic, DateOnly today, DateTimeOffset now)
    {
        var metrics = await LoadTodayMetricsAsync(clinic.Id, today);
        if (metrics is null) return; // aggregation hasn't run yet today

        var plan = await LoadActivePlanAsync(clinic.Id);
        if (plan is null) return; // no active subscription

        var owner = await _db.Users.FirstOrDefaultAsync(u => u.Id == clinic.OwnerUserId);
        if (owner is null) return;

        var checks = BuildLimitChecks(metrics, plan);

        foreach (var check in checks)
            await EvaluateAndNotifyAsync(clinic, owner, check, now);
    }

    // ── Limit evaluation ──────────────────────────────────────────────────────

    /// <summary>
    /// Maps each monthly metric to its plan limit.
    /// Skips unlimited limits (max ≤ 0).
    /// </summary>
    private static LimitCheck[] BuildLimitChecks(ClinicUsageMetrics metrics, SubscriptionPlan plan)
        =>
        [
            new("patients",     metrics.NewPatientsCount,  plan.MaxPatientsPerMonth),
            new("appointments", metrics.AppointmentsCount, plan.MaxAppointmentsPerMonth),
            new("invoices",     metrics.InvoicesCount,     plan.MaxInvoicesPerMonth),
        ];

    private async Task EvaluateAndNotifyAsync(
        Clinic clinic, User owner, LimitCheck check, DateTimeOffset now)
    {
        if (check.Max <= 0) return; // unlimited

        var percent = check.PercentUsed;

        if (percent >= CriticalThresholdPercent)
            await NotifyIfNotAlreadySentAsync(clinic, owner, check, CriticalThresholdPercent, now);
        else if (percent >= WarnThresholdPercent)
            await NotifyIfNotAlreadySentAsync(clinic, owner, check, WarnThresholdPercent, now);
    }

    // ── Notification creation ─────────────────────────────────────────────────

    private async Task NotifyIfNotAlreadySentAsync(
        Clinic clinic, User owner,
        LimitCheck check, int thresholdPercent,
        DateTimeOffset now)
    {
        if (await AlreadySentThisMonthAsync(owner.Id, check.Label, thresholdPercent, now))
            return;

        var isCritical = thresholdPercent >= CriticalThresholdPercent;

        QueueInAppNotification(owner.Id, check, isCritical, now);
        QueueEmail(clinic, owner, check, isCritical);

        _logger.LogInformation(
            "Limit notification queued — clinic {ClinicId}, {Label} at {Percent}% ({Threshold}% threshold)",
            clinic.Id, check.Label, check.PercentUsed, thresholdPercent);
    }

    private void QueueInAppNotification(
        Guid userId, LimitCheck check, bool isCritical, DateTimeOffset now)
    {
        _db.Set<Notification>().Add(new Notification
        {
            UserId    = userId,
            Type      = isCritical ? NotificationType.Error : NotificationType.Warning,
            Title     = BuildTitle(check, isCritical),
            Message   = BuildMessage(check, isCritical),
            ActionUrl = "/usage",
            ExpiresAt = now.AddDays(7),
        });
    }

    private void QueueEmail(Clinic clinic, User owner, LimitCheck check, bool isCritical)
    {
        _db.Set<EmailQueue>().Add(new EmailQueue
        {
            ToEmail  = owner.Email!,
            ToName   = owner.FullName,
            Subject  = $"[{clinic.Name}] {BuildTitle(check, isCritical)}",
            Body     = UsageLimitEmailTemplate.Build(
                           owner.FullName, clinic.Name,
                           check.Label, check.Used, check.Max, check.PercentUsed, isCritical),
            IsHtml   = true,
            Priority = isCritical ? 1 : 2,
        });
    }

    // ── Deduplication ─────────────────────────────────────────────────────────

    /// <summary>
    /// Returns true if a notification for this limit + threshold was already sent this month.
    /// Matches on title content — avoids a separate dedup table.
    /// </summary>
    private async Task<bool> AlreadySentThisMonthAsync(
        Guid userId, string limitLabel, int thresholdPercent, DateTimeOffset now)
    {
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);

        return await _db.Set<Notification>()
            .AnyAsync(n =>
                n.UserId    == userId          &&
                n.CreatedAt >= monthStart      &&
                n.Title.Contains(limitLabel)   &&
                n.Title.Contains(thresholdPercent.ToString()));
    }

    // ── Data loading ──────────────────────────────────────────────────────────

    private Task<List<Clinic>> LoadActiveClinicsAsync()
        => TenantGuard.AsSystemQuery(_db.Set<Clinic>())
            .Where(c => c.IsActive && !c.IsDeleted)
            .ToListAsync();

    private Task<ClinicUsageMetrics?> LoadTodayMetricsAsync(Guid clinicId, DateOnly today)
        => TenantGuard.AsSystemQuery(_db.Set<ClinicUsageMetrics>())
            .FirstOrDefaultAsync(m => m.ClinicId == clinicId && m.MetricDate == today);

    private async Task<SubscriptionPlan?> LoadActivePlanAsync(Guid clinicId)
    {
        var subscription = await TenantGuard.AsSystemQuery(_db.Set<ClinicSubscription>())
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.ClinicId == clinicId &&
                        (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial))
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefaultAsync();

        return subscription?.SubscriptionPlan;
    }

    // ── Text helpers ──────────────────────────────────────────────────────────

    private static string BuildTitle(LimitCheck check, bool isCritical)
        => isCritical
            ? $"Critical: {check.PercentUsed}% of monthly {check.Label} limit used"
            : $"Warning: {check.PercentUsed}% of monthly {check.Label} limit used";

    private static string BuildMessage(LimitCheck check, bool isCritical)
        => $"You have used {check.Used} of {check.Max} {check.Label} this month ({check.PercentUsed}%). " +
           (isCritical
               ? "You are about to reach your plan limit. Consider upgrading your plan."
               : "You are approaching your plan limit.");

    // ── Value object ──────────────────────────────────────────────────────────

    /// <summary>Bundles a single limit check — label, current usage, and plan max.</summary>
    private sealed record LimitCheck(string Label, int Used, int Max)
    {
        public int PercentUsed => Max > 0 ? (int)((double)Used / Max * 100) : 0;
    }
}
