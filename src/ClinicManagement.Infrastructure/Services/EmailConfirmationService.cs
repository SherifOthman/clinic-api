using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Common.Options;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.Services;

public class EmailConfirmationService : IEmailConfirmationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly SmtpEmailSender _emailSender;
    private readonly SmtpOptions _options;
    private readonly ILogger<EmailConfirmationService> _logger;

    public EmailConfirmationService(
        IUnitOfWork unitOfWork,
        ITokenGenerator tokenGenerator,
        SmtpEmailSender emailSender,
        IOptions<SmtpOptions> options,
        ILogger<EmailConfirmationService> logger)
    {
        _unitOfWork = unitOfWork;
        _tokenGenerator = tokenGenerator;
        _emailSender = emailSender;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendConfirmationEmailAsync(User user, CancellationToken cancellationToken = default)
    {
        var token = _tokenGenerator.GenerateEmailConfirmationToken(user.Id, user.Email!);
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
        if (!_tokenGenerator.ValidateEmailConfirmationToken(user.Id, user.Email!, token))
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
}

