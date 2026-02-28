using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

public class UsageMetricsAggregationJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UsageMetricsAggregationJob> _logger;

    public UsageMetricsAggregationJob(
        IServiceProvider serviceProvider,
        ILogger<UsageMetricsAggregationJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Usage Metrics Aggregation Job started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                var nextRun = GetNextRunTime(now);
                var delay = nextRun - now;

                _logger.LogInformation("Next usage metrics aggregation scheduled for {NextRun} UTC", nextRun);
                
                await Task.Delay(delay, stoppingToken);
                
                if (!stoppingToken.IsCancellationRequested)
                {
                    await AggregateUsageMetricsAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Usage Metrics Aggregation Job is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during usage metrics aggregation");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private static DateTime GetNextRunTime(DateTime now)
    {
        var nextRun = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0, DateTimeKind.Utc);
        
        if (now >= nextRun)
        {
            nextRun = nextRun.AddDays(1);
        }
        
        return nextRun;
    }

    private async Task AggregateUsageMetricsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        try
        {
            var yesterday = DateTime.UtcNow.Date.AddDays(-1);
            var activeClinics = await context.Clinics
                .Where(c => c.IsActive && !c.IsDeleted)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Starting usage metrics aggregation for {Count} clinics for date {Date}", 
                activeClinics.Count, yesterday);

            var processedCount = 0;
            var errorCount = 0;

            foreach (var clinic in activeClinics)
            {
                try
                {
                    var metrics = await CalculateMetricsForClinicAsync(
                        context, 
                        clinic.Id, 
                        yesterday, 
                        cancellationToken);

                    var existing = await context.ClinicUsageMetrics
                        .FirstOrDefaultAsync(m => m.ClinicId == clinic.Id && m.MetricDate == yesterday, cancellationToken);

                    if (existing != null)
                    {
                        existing.ActiveStaffCount = metrics.ActiveStaffCount;
                        existing.NewPatientsCount = metrics.NewPatientsCount;
                        existing.TotalPatientsCount = metrics.TotalPatientsCount;
                        existing.AppointmentsCount = metrics.AppointmentsCount;
                        existing.InvoicesCount = metrics.InvoicesCount;
                        existing.StorageUsedGB = metrics.StorageUsedGB;
                        existing.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        context.ClinicUsageMetrics.Add(metrics);
                    }

                    await context.SaveChangesAsync(cancellationToken);
                    processedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error aggregating metrics for clinic {ClinicId}", clinic.Id);
                    errorCount++;
                }
            }

            _logger.LogInformation(
                "Usage metrics aggregation completed: {ProcessedCount} clinics processed, {ErrorCount} errors", 
                processedCount, errorCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error during usage metrics aggregation");
            throw;
        }
    }

    private async Task<ClinicUsageMetrics> CalculateMetricsForClinicAsync(
        IApplicationDbContext context,
        Guid clinicId,
        DateTime metricDate,
        CancellationToken cancellationToken)
    {
        var activeStaffCount = await context.Staff
            .CountAsync(s => s.ClinicId == clinicId && 
                           s.IsActive && 
                           s.Status == StaffStatus.Active, 
                       cancellationToken);

        var newPatientsCount = 0;
        var totalPatientsCount = 0;
        var appointmentsCount = 0;
        var invoicesCount = 0;
        var storageUsedGB = 0m;

        var metrics = new ClinicUsageMetrics
        {
            ClinicId = clinicId,
            MetricDate = metricDate,
            ActiveStaffCount = activeStaffCount,
            NewPatientsCount = newPatientsCount,
            TotalPatientsCount = totalPatientsCount,
            AppointmentsCount = appointmentsCount,
            InvoicesCount = invoicesCount,
            StorageUsedGB = storageUsedGB,
            UpdatedAt = DateTime.UtcNow
        };

        return metrics;
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Usage Metrics Aggregation Job is stopping");
        await base.StopAsync(stoppingToken);
    }
}
