using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

public class EmailQueueProcessorJob
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailQueueProcessorJob> _logger;

    public EmailQueueProcessorJob(
        ApplicationDbContext context,
        IEmailService emailService,
        ILogger<EmailQueueProcessorJob> logger)
    {
        _context      = context;
        _emailService = emailService;
        _logger       = logger;
    }

    public async Task ExecuteAsync()
    {
        var pendingEmails = await _context.Set<EmailQueue>()
            .Where(e => e.Status == EmailQueueStatus.Pending && e.Attempts < e.MaxAttempts)
            .OrderBy(e => e.Priority).ThenBy(e => e.CreatedAt)
            .Take(50)
            .ToListAsync();

        if (!pendingEmails.Any()) return;

        _logger.LogInformation("Processing {Count} pending emails", pendingEmails.Count);

        var sentCount = 0; var failedCount = 0;

        foreach (var email in pendingEmails)
        {
            try
            {
                email.Status = EmailQueueStatus.Sending;
                email.Attempts++;
                await _context.SaveChangesAsync();

                await _emailService.SendEmailAsync(email.ToEmail, email.ToName, email.Subject, email.Body, email.IsHtml);

                email.Status       = EmailQueueStatus.Sent;
                email.SentAt       = DateTimeOffset.UtcNow;
                email.ErrorMessage = null;
                await _context.SaveChangesAsync();
                sentCount++;
            }
            catch (Exception ex)
            {
                email.Status       = email.Attempts >= email.MaxAttempts ? EmailQueueStatus.Failed : EmailQueueStatus.Pending;
                email.ErrorMessage = ex.Message;
                await _context.SaveChangesAsync();
                failedCount++;
                _logger.LogWarning(ex, "Failed to send email to {ToEmail} (Attempt {A}/{Max})",
                    email.ToEmail, email.Attempts, email.MaxAttempts);
            }
        }

        _logger.LogInformation("Email queue: {Sent} sent, {Failed} failed", sentCount, failedCount);
    }
}
