using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Application.Common.Extensions;
using ClinicManagement.Application.Common.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

public class EmailConfirmationService
{
    private readonly UserManager<User> _userManager;
    private readonly SmtpEmailSender _emailSender;
    private readonly SmtpOptions _options;
    private readonly ILogger<EmailConfirmationService> _logger;

    public EmailConfirmationService(
        UserManager<User> userManager,
        SmtpEmailSender emailSender,
        IOptions<SmtpOptions> options,
        ILogger<EmailConfirmationService> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendConfirmationEmailAsync(User user, CancellationToken cancellationToken = default)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = $"{_options.FrontendUrl}/confirm-email?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";
        
        var emailBody = EmailTemplates.GetEmailConfirmationTemplate($"{user.FirstName} {user.LastName}".Trim(), confirmationLink);
        
        await _emailSender.SendEmailAsync(
            user.Email!,
            "Confirm your email address",
            emailBody,
            cancellationToken);
    }

    public async Task ConfirmEmailAsync(User user, string token, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.ConfirmEmailAsync(user, token);
        
        if (!result.Succeeded)
        {
            _logger.LogWarning("Email confirmation failed for {Email}. Errors: {Errors}", 
                user.Email, 
                string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}")));
        }
        
        result.ThrowIfFailed();
    }

    public async Task<bool> IsEmailConfirmedAsync(User user, CancellationToken cancellationToken = default)
    {
        return await _userManager.IsEmailConfirmedAsync(user);
    }
}
