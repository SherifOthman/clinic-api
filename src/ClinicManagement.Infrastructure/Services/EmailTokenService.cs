using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Common.Options;
using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.Services;

public class EmailTokenService : IEmailTokenService
{
    private readonly UserManager<User> _userManager;
    private readonly SmtpEmailSender   _emailSender;
    private readonly AppOptions        _appOptions;
    private readonly ILogger<EmailTokenService> _logger;

    public EmailTokenService(
        UserManager<User> userManager,
        SmtpEmailSender emailSender,
        IOptions<AppOptions> appOptions,
        ILogger<EmailTokenService> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _appOptions  = appOptions.Value;
        _logger      = logger;
    }

    public async Task SendConfirmationEmailAsync(User user, CancellationToken cancellationToken = default)
    {
        var token            = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = $"{_appOptions.FrontendUrl}/confirm-email?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";
        var emailBody        = EmailTemplates.GetEmailConfirmationTemplate(user.FullName, confirmationLink);

        await _emailSender.SendEmailAsync(user.Email!, "Confirm your email address", emailBody, cancellationToken);
    }

    public async Task ConfirmEmailAsync(User user, string token, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Invalid email confirmation token for {Email}: {Errors}", user.Email, errors);
            throw new InvalidOperationException("Invalid or expired confirmation token");
        }

        _logger.LogInformation("Email confirmed successfully for {Email}", user.Email);
    }

    public async Task<bool> IsEmailConfirmedAsync(User user, CancellationToken cancellationToken = default)
        => await _userManager.IsEmailConfirmedAsync(user);

    public string GeneratePasswordResetToken(Guid userId, string email, string passwordHash)
        => throw new NotSupportedException("Use UserManager.GeneratePasswordResetTokenAsync instead");

    public bool ValidatePasswordResetToken(Guid userId, string email, string passwordHash, string token)
        => throw new NotSupportedException("Use UserManager.ResetPasswordAsync instead");
}
