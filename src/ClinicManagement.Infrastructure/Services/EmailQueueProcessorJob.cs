using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

public class EmailQueueProcessorJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailQueueProcessorJob> _logger;

    public EmailQueueProcessorJob(
        IServiceProvider serviceProvider,
        ILogger<EmailQueueProcessorJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
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
                {
                    await ProcessEmailQueueAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Email Queue Processor Job is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during email queue processing");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    private async Task ProcessEmailQueueAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        try
        {
            var pendingEmails = await unitOfWork.EmailQueue.GetPendingEmailsAsync(50, cancellationToken);
            var emailList = pendingEmails.ToList();

            if (!emailList.Any())
            {
                _logger.LogDebug("No pending emails to process");
                return;
            }

            _logger.LogInformation("Processing {Count} pending emails", emailList.Count);

            var sentCount = 0;
            var failedCount = 0;

            foreach (var email in emailList)
            {
                try
                {
                    // Update status to Sending and increment attempts
                    email.Status = EmailQueueStatus.Sending;
                    email.Attempts++;
                    await unitOfWork.EmailQueue.UpdateAsync(email, cancellationToken);

                    // Send the email using the generic send method
                    await SendEmailAsync(emailService, email, cancellationToken);

                    // Mark as sent
                    email.Status = EmailQueueStatus.Sent;
                    email.SentAt = DateTime.UtcNow;
                    email.ErrorMessage = null;
                    await unitOfWork.EmailQueue.UpdateAsync(email, cancellationToken);

                    sentCount++;
                    _logger.LogDebug("Email sent successfully to {ToEmail}", email.ToEmail);
                }
                catch (Exception ex)
                {
                    // Determine if we should retry or mark as failed
                    email.Status = email.Attempts >= email.MaxAttempts ? EmailQueueStatus.Failed : EmailQueueStatus.Pending;
                    email.ErrorMessage = ex.Message;
                    await unitOfWork.EmailQueue.UpdateAsync(email, cancellationToken);

                    failedCount++;
                    _logger.LogWarning(ex, 
                        "Failed to send email to {ToEmail} (Attempt {Attempts}/{MaxAttempts})", 
                        email.ToEmail, email.Attempts, email.MaxAttempts);
                }
            }

            _logger.LogInformation(
                "Email queue processing completed: {SentCount} sent, {FailedCount} failed", 
                sentCount, failedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error during email queue processing");
            throw;
        }
    }

    private async Task SendEmailAsync(
        IEmailService emailService, 
        Domain.Entities.EmailQueue email, 
        CancellationToken cancellationToken)
    {
        await emailService.SendEmailAsync(
            email.ToEmail,
            email.ToName,
            email.Subject,
            email.Body,
            email.IsHtml,
            cancellationToken);
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Queue Processor Job is stopping");
        await base.StopAsync(stoppingToken);
    }
}
