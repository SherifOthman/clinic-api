using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.ResendEmailVerification;

public class ResendEmailVerificationHandler : IRequestHandler<ResendEmailVerificationCommand, ResendEmailVerificationResult>
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailConfirmationService _emailConfirmationService;
    private readonly ILogger<ResendEmailVerificationHandler> _logger;

    public ResendEmailVerificationHandler(
        UserManager<User> userManager,
        IEmailConfirmationService emailConfirmationService,
        ILogger<ResendEmailVerificationHandler> logger)
    {
        _userManager = userManager;
        _emailConfirmationService = emailConfirmationService;
        _logger = logger;
    }

    public async Task<ResendEmailVerificationResult> Handle(
        ResendEmailVerificationCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Resend email verification attempted for non-existent user: {Email}", request.Email);
            return new ResendEmailVerificationResult(
                Success: false,
                ErrorCode: ErrorCodes.USER_NOT_FOUND,
                ErrorMessage: "User not found"
            );
        }

        if (await _emailConfirmationService.IsEmailConfirmedAsync(user, cancellationToken))
        {
            _logger.LogInformation("Resend email verification attempted for already confirmed email: {Email}", request.Email);
            return new ResendEmailVerificationResult(
                Success: false,
                ErrorCode: ErrorCodes.EMAIL_ALREADY_CONFIRMED,
                ErrorMessage: "Email is already confirmed"
            );
        }

        try
        {
            await _emailConfirmationService.SendConfirmationEmailAsync(user, cancellationToken);
            _logger.LogInformation("Verification email resent to: {Email}", request.Email);
            
            return new ResendEmailVerificationResult(
                Success: true,
                ErrorCode: null,
                ErrorMessage: null
            );
        }
        catch (DomainException ex)
        {
            _logger.LogError(ex, "Failed to resend verification email to: {Email}", request.Email);
            return new ResendEmailVerificationResult(
                Success: false,
                ErrorCode: ex.ErrorCode,
                ErrorMessage: ex.Message
            );
        }
    }
}
