using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Templates;
using ClinicManagement.Application.Options;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Application.Features.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand : IRequest<Result>
{
    public string Email { get; init; } = string.Empty;
}

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IUserManagementService _userManagementService;
    private readonly IEmailSender _emailSender;
    private readonly SmtpOptions _smtpOptions;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        IUserManagementService userManagementService,
        IEmailSender emailSender,
        IOptions<SmtpOptions> smtpOptions,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _userManagementService = userManagementService;
        _emailSender = emailSender;
        _smtpOptions = smtpOptions.Value;
        _logger = logger;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManagementService.GetUserByEmailAsync(request.Email, cancellationToken);
        
        // Always return success to prevent email enumeration
        if (user == null)
        {
            _logger.LogWarning("Password reset requested for non-existent email: {Email}", request.Email);
            return Result.Ok();
        }

        // Generate password reset token
        var token = await _userManagementService.GeneratePasswordResetTokenAsync(user, cancellationToken);
        
        // Create reset link
        var resetLink = $"{_smtpOptions.FrontendUrl}/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";
        
        // Send email
        var emailBody = EmailTemplates.GetPasswordResetTemplate($"{user.FirstName} {user.LastName}".Trim(), resetLink);
        
        try
        {
            await _emailSender.SendEmailAsync(
                user.Email!,
                "Reset your password",
                emailBody,
                cancellationToken);
            
            _logger.LogInformation("Password reset email sent to: {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to: {Email}", user.Email);
            // Still return success to prevent email enumeration
        }

        return Result.Ok();
    }
}