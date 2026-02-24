using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Domain.Repositories;
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
        // Schedule to run daily at 1:00 AM UTC
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
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        try
        {
            var yesterday = DateTime.UtcNow.Date.AddDays(-1);
            var clinics = await unitOfWork.Clinics.GetAllAsync(cancellationToken);
            var activeClinics = clinics.Where(c => c.IsActive && !c.IsDeleted).ToList();

            _logger.LogInformation("Starting usage metrics aggregation for {Count} clinics for date {Date}", 
                activeClinics.Count, yesterday);

            var processedCount = 0;
            var errorCount = 0;

            foreach (var clinic in activeClinics)
            {
                try
                {
                    var metrics = await CalculateMetricsForClinicAsync(
                        unitOfWork, 
                        clinic.Id, 
                        yesterday, 
                        cancellationToken);

                    await unitOfWork.ClinicUsageMetrics.UpsertAsync(metrics, cancellationToken);
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
        IUnitOfWork unitOfWork,
        Guid clinicId,
        DateTime metricDate,
        CancellationToken cancellationToken)
    {
        // Get all staff for the clinic
        var allStaff = await unitOfWork.Staff.GetByClinicIdAsync(clinicId, cancellationToken);
        var activeStaffCount = allStaff.Count(s => 
            s.IsActive && 
            s.Status == StaffStatus.Active);

        // For now, we'll set these to 0 since Patient, Appointment, and Invoice repositories
        // are not yet available in the IUnitOfWork interface
        // TODO: Add these repositories to IUnitOfWork and implement proper counting
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
