using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.ConfirmEmail;

/// <summary>
/// Legacy handler — kept for backward compatibility with the existing
/// POST /api/auth/confirm-email endpoint (UserId + token from Identity).
/// New clients should use POST /api/auth/verify-email-otp (email + 6-digit OTP).
/// </summary>
public class ConfirmEmailHandler : IRequestHandler<ConfirmEmailCommand, Result>
{
    private readonly IUserRepository     _users;
    private readonly IEmailTokenService  _emailTokenService;
    private readonly ILogger<ConfirmEmailHandler> _logger;

    public ConfirmEmailHandler(
        IUserRepository users,
        IEmailTokenService emailTokenService,
        ILogger<ConfirmEmailHandler> logger)
    {
        _users             = users;
        _emailTokenService = emailTokenService;
        _logger            = logger;
    }

    public async Task<Result> Handle(ConfirmEmailCommand request, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(request.UserId, ct);

        if (user is null)
        {
            _logger.LogWarning("Email confirmation attempted for non-existent user: {UserId}", request.UserId);
            return Result.Failure(ErrorCodes.USER_NOT_FOUND, "User not found");
        }

        if (await _emailTokenService.IsEmailConfirmedAsync(user, ct))
        {
            _logger.LogInformation("Email already confirmed for user: {UserId}", user.Id);
            return Result.Failure(ErrorCodes.EMAIL_ALREADY_CONFIRMED, "Email is already confirmed");
        }

        // Treat the token as an OTP — verify via the OTP service
        var verified = await _emailTokenService.VerifyConfirmationOtpAsync(user, request.Token, ct);
        if (!verified)
        {
            _logger.LogWarning("Email confirmation failed for user {UserId}", user.Id);
            return Result.Failure(ErrorCodes.TOKEN_INVALID, "Invalid or expired code");
        }

        _logger.LogInformation("Email confirmed successfully for user: {UserId}", user.Id);
        return Result.Success();
    }
}
