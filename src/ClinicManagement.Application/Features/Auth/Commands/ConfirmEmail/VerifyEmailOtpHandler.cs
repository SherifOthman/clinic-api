using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.ConfirmEmail;

public class VerifyEmailOtpHandler : IRequestHandler<VerifyEmailOtpCommand, Result>
{
    private readonly IUserRepository     _users;
    private readonly IEmailTokenService  _emailTokenService;
    private readonly ILogger<VerifyEmailOtpHandler> _logger;

    public VerifyEmailOtpHandler(
        IUserRepository users,
        IEmailTokenService emailTokenService,
        ILogger<VerifyEmailOtpHandler> logger)
    {
        _users             = users;
        _emailTokenService = emailTokenService;
        _logger            = logger;
    }

    public async Task<Result> Handle(VerifyEmailOtpCommand request, CancellationToken ct)
    {
        var user = await _users.GetByEmailOrUsernameAsync(request.Email, ct);
        if (user is null)
        {
            _logger.LogWarning("Email OTP verification for non-existent email: {Email}", request.Email);
            // Return same error as invalid OTP to prevent email enumeration
            return Result.Failure(ErrorCodes.TOKEN_INVALID, "Invalid or expired code");
        }

        if (await _emailTokenService.IsEmailConfirmedAsync(user, ct))
            return Result.Failure(ErrorCodes.EMAIL_ALREADY_CONFIRMED, "Email is already confirmed");

        var verified = await _emailTokenService.VerifyConfirmationOtpAsync(user, request.Otp, ct);
        if (!verified)
            return Result.Failure(ErrorCodes.TOKEN_INVALID, "Invalid or expired code");

        _logger.LogInformation("Email confirmed via OTP for {Email}", request.Email);
        return Result.Success();
    }
}
