using ClinicManagement.Infrastructure.Services;
using ClinicManagement.Persistence.Jobs;
using Hangfire;

namespace ClinicManagement.API.Hangfire;

/// <summary>
/// Registers all Hangfire recurring jobs in one place.
///
/// Uses IRecurringJobManager (DI-based) instead of the static RecurringJob API.
/// The static API requires JobStorage.Current to be set, which only happens after
/// the Hangfire server starts — using the DI interface avoids that timing issue.
/// </summary>
public static class HangfireJobRegistration
{
    private const string Every5Minutes = "*/5 * * * *";
    private const string Every6Hours   = "0 */6 * * *";
    private const string DailyMidnight = "0 0 * * *";
    private const string DailyAt1Am    = "0 1 * * *";
    private const string DailyAt9Am    = "0 9 * * *";

    public static void RegisterAll(IRecurringJobManager jobs)
    {
        jobs.AddOrUpdate<EmailQueueProcessorJob>           (nameof(EmailQueueProcessorJob),            j => j.ExecuteAsync(), Every5Minutes);
        jobs.AddOrUpdate<RefreshTokenCleanupService>       (nameof(RefreshTokenCleanupService),        j => j.ExecuteAsync(), Every6Hours);
        jobs.AddOrUpdate<AuditLogCleanupService>           (nameof(AuditLogCleanupService),            j => j.ExecuteAsync(), DailyMidnight);
        jobs.AddOrUpdate<UsageMetricsAggregationJob>       (nameof(UsageMetricsAggregationJob),        j => j.ExecuteAsync(), DailyAt1Am);
        jobs.AddOrUpdate<SubscriptionExpiryNotificationJob>(nameof(SubscriptionExpiryNotificationJob), j => j.ExecuteAsync(), DailyAt9Am);
        jobs.AddOrUpdate<UsageLimitNotificationJob>        (nameof(UsageLimitNotificationJob),         j => j.ExecuteAsync(), DailyAt9Am);
    }
}
