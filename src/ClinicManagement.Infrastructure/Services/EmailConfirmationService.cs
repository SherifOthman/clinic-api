using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Templates;
using ClinicManagement.Application.Options;
using ClinicManagement.Infrastructure.Common.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

public class EmailConfirmationService : IEmailConfirmationService
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly SmtpOptions _options;
    private readonly ILogger<EmailConfirmationService> _logger;

    public EmailConfirmationService(
        UserManager<User> userManager,
        IEmailSender emailSender,
        IOptions<SmtpOptions> options,
        ILogger<EmailConfirmationService> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result> SendConfirmationEmailAsync(User user, CancellationToken cancellationToken = default)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = $"{_options.FrontendUrl}/confirm-email?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";
        
        var emailBody = EmailTemplates.GetEmailConfirmationTemplate($"{user.FirstName} {user.LastName}".Trim(), confirmationLink);
        
        await _emailSender.SendEmailAsync(
            user.Email!,
            "Confirm your email address",
            emailBody,
            cancellationToken);

        return Result.Ok();
    }

    public async Task<Result> ConfirmEmailAsync(User user, string token, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.ConfirmEmailAsync(user, token);
        
        if (!result.Succeeded)
        {
            _logger.LogWarning("Email confirmation failed for {Email}. Errors: {Errors}", 
                user.Email, 
                string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}")));
        }
        
        return result.ToResult();
    }

    public async Task<bool> IsEmailConfirmedAsync(User user, CancellationToken cancellationToken = default)
    {
        return await _userManager.IsEmailConfirmedAsync(user);
    }
}
