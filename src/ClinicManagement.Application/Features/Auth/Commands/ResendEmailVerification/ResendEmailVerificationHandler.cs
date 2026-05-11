using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.ResendEmailVerification;

public class ResendEmailVerificationHandler : IRequestHandler<ResendEmailVerificationCommand, Result>
{
    private readonly IUserRepository    _users;
    private readonly IEmailTokenService _emailTokenService;
    private readonly ILogger<ResendEmailVerificationHandler> _logger;

    public ResendEmailVerificationHandler(
        IUserRepository users,
        IEmailTokenService emailTokenService,
        ILogger<ResendEmailVerificationHandler> logger)
    {
        _users             = users;
        _emailTokenService = emailTokenService;
        _logger            = logger;
    }

    public async Task<Result> Handle(ResendEmailVerificationCommand request, CancellationToken ct)
    {
        var user = await _users.GetByEmailOrUsernameAsync(request.Email, ct);

        if (user is null)
        {
            _logger.LogInformation("Resend OTP requested for non-existent email: {Email}", request.Email);
            return Result.Success(); // no enumeration
        }

        if (user.EmailConfirmed)
            return Result.Failure(ErrorCodes.EMAIL_ALREADY_CONFIRMED, "Email is already confirmed");

        try
        {
            await _emailTokenService.SendConfirmationOtpAsync(user, ct);
            _logger.LogInformation("Verification OTP resent to: {Email}", request.Email);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resend verification OTP to: {Email}", request.Email);
            return Result.Failure(ErrorCodes.OPERATION_FAILED, "Failed to send verification code");
        }
    }
}
