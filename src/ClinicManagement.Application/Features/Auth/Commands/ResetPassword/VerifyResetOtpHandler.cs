using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.ResetPassword;

public class VerifyResetOtpHandler : IRequestHandler<VerifyResetOtpCommand, Result>
{
    private readonly IUserRepository    _users;
    private readonly IEmailTokenService _emailTokenService;
    private readonly ILogger<VerifyResetOtpHandler> _logger;

    public VerifyResetOtpHandler(
        IUserRepository users,
        IEmailTokenService emailTokenService,
        ILogger<VerifyResetOtpHandler> logger)
    {
        _users             = users;
        _emailTokenService = emailTokenService;
        _logger            = logger;
    }

    public async Task<Result> Handle(VerifyResetOtpCommand request, CancellationToken ct)
    {
        var user = await _users.GetByEmailOrUsernameAsync(request.Email, ct);
        if (user is null)
        {
            _logger.LogWarning("Reset OTP verification for non-existent email: {Email}", request.Email);
            return Result.Failure(ErrorCodes.TOKEN_INVALID, "Invalid or expired code");
        }

        if (!await _emailTokenService.ValidatePasswordResetOtpAsync(user, request.Otp, ct))
            return Result.Failure(ErrorCodes.TOKEN_INVALID, "Invalid or expired code");

        _logger.LogInformation("Password reset OTP verified for {Email}", request.Email);
        return Result.Success();
    }
}