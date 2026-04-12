using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

public class EmailQueueProcessorJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailQueueProcessorJob> _logger;

    public EmailQueueProcessorJob(IServiceProvider serviceProvider, ILogger<EmailQueueProcessorJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger          = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Queue Processor Job started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                if (!stoppingToken.IsCancellationRequested)
                    await ProcessEmailQueueAsync(stoppingToken);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email queue processing");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    private async Task ProcessEmailQueueAsync(CancellationToken cancellationToken)
    {
        using var scope    = _serviceProvider.CreateScope();
        var context        = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var emailQueue     = context.Set<EmailQueue>();
        var emailService   = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var pendingEmails = await emailQueue
            .Where(e => e.Status == EmailQueueStatus.Pending && e.Attempts < e.MaxAttempts)
            .OrderBy(e => e.Priority).ThenBy(e => e.CreatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        if (!pendingEmails.Any()) return;

        _logger.LogInformation("Processing {Count} pending emails", pendingEmails.Count);

        var sentCount = 0; var failedCount = 0;

        foreach (var email in pendingEmails)
        {
            try
            {
                email.Status = EmailQueueStatus.Sending;
                email.Attempts++;
                await context.SaveChangesAsync(cancellationToken);

                await emailService.SendEmailAsync(email.ToEmail, email.ToName, email.Subject, email.Body, email.IsHtml, cancellationToken);

                email.Status = EmailQueueStatus.Sent;
                email.SentAt = DateTimeOffset.UtcNow;
                email.ErrorMessage = null;
                await context.SaveChangesAsync(cancellationToken);
                sentCount++;
            }
            catch (Exception ex)
            {
                email.Status       = email.Attempts >= email.MaxAttempts ? EmailQueueStatus.Failed : EmailQueueStatus.Pending;
                email.ErrorMessage = ex.Message;
                await context.SaveChangesAsync(cancellationToken);
                failedCount++;
                _logger.LogWarning(ex, "Failed to send email to {ToEmail} (Attempt {A}/{Max})", email.ToEmail, email.Attempts, email.MaxAttempts);
            }
        }

        _logger.LogInformation("Email queue: {Sent} sent, {Failed} failed", sentCount, failedCount);
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Queue Processor Job stopping");
        await base.StopAsync(stoppingToken);
    }
}
