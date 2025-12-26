using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.ResendEmailVerification;

public class ResendEmailVerificationCommandHandler : IRequestHandler<ResendEmailVerificationCommand, Result>
{
    private readonly IIdentityService _identityService;

    public ResendEmailVerificationCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> Handle(ResendEmailVerificationCommand request, CancellationToken cancellationToken)
    {
        // Get user by email
        var user = await _identityService.GetUserByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            // Don't reveal if email exists or not for security
            return Result.Ok("If the email exists in our system, a verification email has been sent.");
        }

        // Check if email is already confirmed
        if (await _identityService.IsEmailConfirmedAsync(user, cancellationToken))
        {
            return Result.Ok("Email is already verified.");
        }

        // Use the centralized email confirmation method - let GlobalExceptionMiddleware handle failures
        await _identityService.SendConfirmationEmailAsync(user, cancellationToken);

        return Result.Ok("Verification email has been sent. Please check your inbox.");
    }
}