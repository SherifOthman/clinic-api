using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Jobs;

public class EmailQueueProcessorJob
{
    private readonly ApplicationDbContext _db;
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailQueueProcessorJob> _logger;

    public EmailQueueProcessorJob(
        ApplicationDbContext db,
        IEmailService emailService,
        ILogger<EmailQueueProcessorJob> logger)
    {
        _db           = db;
        _emailService = emailService;
        _logger       = logger;
    }

    public async Task ExecuteAsync()
    {
        var pending = await _db.Set<EmailQueue>()
            .Where(e => e.Status == EmailQueueStatus.Pending && e.Attempts < e.MaxAttempts)
            .OrderBy(e => e.Priority).ThenBy(e => e.CreatedAt)
            .Take(50)
            .ToListAsync();

        if (!pending.Any()) return;

        _logger.LogInformation("Processing {Count} pending emails", pending.Count);

        var sent = 0; var failed = 0;

        foreach (var email in pending)
        {
            try
            {
                // Mark as Sending before attempting — prevents another job instance
                // from picking up the same email if this one takes too long.
                email.Status = EmailQueueStatus.Sending;
                email.Attempts++;
                await _db.SaveChangesAsync();

                await _emailService.SendEmailAsync(email.ToEmail, email.ToName, email.Subject, email.Body, email.IsHtml);

                email.Status       = EmailQueueStatus.Sent;
                email.SentAt       = DateTimeOffset.UtcNow;
                email.ErrorMessage = null;
                await _db.SaveChangesAsync();
                sent++;
            }
            catch (Exception ex)
            {
                email.Status       = email.Attempts >= email.MaxAttempts ? EmailQueueStatus.Failed : EmailQueueStatus.Pending;
                email.ErrorMessage = ex.Message;
                await _db.SaveChangesAsync();
                failed++;
                _logger.LogWarning(ex, "Failed to send email to {ToEmail} (attempt {A}/{Max})",
                    email.ToEmail, email.Attempts, email.MaxAttempts);
            }
        }

        _logger.LogInformation("Email queue: {Sent} sent, {Failed} failed", sent, failed);
    }
}
