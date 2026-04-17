using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

public class UsageMetricsAggregationJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UsageMetricsAggregationJob> _logger;

    public UsageMetricsAggregationJob(IServiceProvider serviceProvider, ILogger<UsageMetricsAggregationJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger          = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Usage Metrics Aggregation Job started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now     = DateTimeOffset.UtcNow;
                var nextRun = GetNextRunTime(now);
                _logger.LogInformation("Next usage metrics aggregation scheduled for {NextRun} UTC", nextRun);
                await Task.Delay(nextRun - now, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                    await AggregateUsageMetricsAsync(stoppingToken);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during usage metrics aggregation");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private static DateTimeOffset GetNextRunTime(DateTimeOffset now)
    {
        var next = new DateTimeOffset(now.Year, now.Month, now.Day, 1, 0, 0, TimeSpan.Zero);
        return now >= next ? next.AddDays(1) : next;
    }

    private async Task AggregateUsageMetricsAsync(CancellationToken cancellationToken)
    {
        using var scope      = _serviceProvider.CreateScope();
        var context          = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var clinics          = context.Set<Clinic>();
        var members          = context.Set<ClinicMember>();
        var usageMetrics     = context.Set<ClinicUsageMetrics>();

        var yesterday    = DateOnly.FromDateTime(DateTimeOffset.UtcNow.AddDays(-1).Date);
        var activeClinics = await clinics
            .Where(c => c.IsActive && !c.IsDeleted)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Aggregating metrics for {Count} clinics for {Date}", activeClinics.Count, yesterday);

        var processed = 0; var errors = 0;

        foreach (var clinic in activeClinics)
        {
            try
            {
                var activeStaffCount = await members.CountAsync(m => m.ClinicId == clinic.Id && m.IsActive, cancellationToken);

                var metrics = new ClinicUsageMetrics
                {
                    ClinicId           = clinic.Id,
                    MetricDate         = yesterday,
                    ActiveStaffCount   = activeStaffCount,
                    NewPatientsCount   = 0,
                    TotalPatientsCount = 0,
                    AppointmentsCount  = 0,
                    InvoicesCount      = 0,
                    StorageUsedGB      = 0,
                    UpdatedAt          = DateTimeOffset.UtcNow,
                };

                var existing = await usageMetrics
                    .FirstOrDefaultAsync(m => m.ClinicId == clinic.Id && m.MetricDate == yesterday, cancellationToken);

                if (existing is not null)
                {
                    existing.ActiveStaffCount = metrics.ActiveStaffCount;
                    existing.UpdatedAt        = metrics.UpdatedAt;
                }
                else
                {
                    usageMetrics.Add(metrics);
                }

                await context.SaveChangesAsync(cancellationToken);
                processed++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error aggregating metrics for clinic {ClinicId}", clinic.Id);
                errors++;
            }
        }

        _logger.LogInformation("Usage metrics aggregation: {Processed} processed, {Errors} errors", processed, errors);
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Usage Metrics Aggregation Job stopping");
        await base.StopAsync(stoppingToken);
    }
}
