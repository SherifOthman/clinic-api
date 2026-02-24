using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Common.Options;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.Services;

public class EmailTokenService : IEmailTokenService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly SmtpEmailSender _emailSender;
    private readonly SmtpOptions _options;
    private readonly ILogger<EmailTokenService> _logger;
    private readonly IDataProtector _emailProtector;
    private readonly IDataProtector _passwordProtector;
    private readonly TimeSpan _tokenLifetime = TimeSpan.FromHours(24);

    public EmailTokenService(
        IUnitOfWork unitOfWork,
        SmtpEmailSender emailSender,
        IOptions<SmtpOptions> options,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<EmailTokenService> logger)
    {
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
        _options = options.Value;
        _logger = logger;
        _emailProtector = dataProtectionProvider.CreateProtector("EmailConfirmation");
        _passwordProtector = dataProtectionProvider.CreateProtector("PasswordReset");
    }

    public async Task SendConfirmationEmailAsync(User user, CancellationToken cancellationToken = default)
    {
        var token = GenerateEmailConfirmationToken(user.Id, user.Email!);
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
        if (!ValidateEmailConfirmationToken(user.Id, user.Email!, token))
        {
            _logger.LogWarning("Invalid email confirmation token for {Email}", user.Email);
            throw new InvalidOperationException("Invalid or expired confirmation token");
        }

        user.IsEmailConfirmed = true;
        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        _logger.LogInformation("Email confirmed successfully for {Email}", user.Email);
    }

    public async Task<bool> IsEmailConfirmedAsync(User user, CancellationToken cancellationToken = default)
    {
        return user.IsEmailConfirmed;
    }

    public string GeneratePasswordResetToken(Guid userId, string email, string passwordHash)
    {
        var payload = $"{userId}|{email}|{passwordHash}|{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        return _passwordProtector.Protect(payload);
    }

    public bool ValidatePasswordResetToken(Guid userId, string email, string passwordHash, string token)
    {
        try
        {
            var payload = _passwordProtector.Unprotect(token);
            var parts = payload.Split('|');
            
            if (parts.Length != 4)
                return false;

            var tokenUserId = Guid.Parse(parts[0]);
            var tokenEmail = parts[1];
            var tokenPasswordHash = parts[2];
            var timestamp = long.Parse(parts[3]);
            
            var tokenTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
            var isExpired = DateTimeOffset.UtcNow - tokenTime > _tokenLifetime;

            return tokenUserId == userId && 
                   tokenEmail.Equals(email, StringComparison.OrdinalIgnoreCase) && 
                   tokenPasswordHash == passwordHash &&
                   !isExpired;
        }
        catch
        {
            return false;
        }
    }

    private string GenerateEmailConfirmationToken(Guid userId, string email)
    {
        var payload = $"{userId}|{email}|{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        return _emailProtector.Protect(payload);
    }

    private bool ValidateEmailConfirmationToken(Guid userId, string email, string token)
    {
        try
        {
            var payload = _emailProtector.Unprotect(token);
            var parts = payload.Split('|');
            
            if (parts.Length != 3)
                return false;

            var tokenUserId = Guid.Parse(parts[0]);
            var tokenEmail = parts[1];
            var timestamp = long.Parse(parts[2]);
            
            var tokenTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
            var isExpired = DateTimeOffset.UtcNow - tokenTime > _tokenLifetime;

            return tokenUserId == userId && 
                   tokenEmail.Equals(email, StringComparison.OrdinalIgnoreCase) && 
                   !isExpired;
        }
        catch
        {
            return false;
        }
    }
}

