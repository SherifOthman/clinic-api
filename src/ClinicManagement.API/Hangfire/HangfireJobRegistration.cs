using ClinicManagement.Infrastructure.Services;
using ClinicManagement.Persistence.Jobs;
using Hangfire;

namespace ClinicManagement.API.Hangfire;

/// <summary>
/// Registers all Hangfire recurring jobs in one place.
/// Called from Program.cs only if Hangfire is configured (connection string present).
/// </summary>
public static class HangfireJobRegistration
{
    // Readable cron constants — no need to memorise cron syntax
    private const string Every5Minutes = "*/5 * * * *";
    private const string Every6Hours   = "0 */6 * * *";
    private const string DailyMidnight = "0 0 * * *";
    private const string DailyAt1Am    = "0 1 * * *";
    private const string DailyAt9Am    = "0 9 * * *";

    public static void RegisterAll()
    {
        RecurringJob.AddOrUpdate<EmailQueueProcessorJob>           (nameof(EmailQueueProcessorJob),            j => j.ExecuteAsync(), Every5Minutes);
        RecurringJob.AddOrUpdate<RefreshTokenCleanupService>       (nameof(RefreshTokenCleanupService),        j => j.ExecuteAsync(), Every6Hours);
        RecurringJob.AddOrUpdate<AuditLogCleanupService>           (nameof(AuditLogCleanupService),            j => j.ExecuteAsync(), DailyMidnight);
        RecurringJob.AddOrUpdate<UsageMetricsAggregationJob>       (nameof(UsageMetricsAggregationJob),        j => j.ExecuteAsync(), DailyAt1Am);
        RecurringJob.AddOrUpdate<SubscriptionExpiryNotificationJob>(nameof(SubscriptionExpiryNotificationJob), j => j.ExecuteAsync(), DailyAt9Am);
        RecurringJob.AddOrUpdate<UsageLimitNotificationJob>        (nameof(UsageLimitNotificationJob),         j => j.ExecuteAsync(), DailyAt9Am);
    }
}
