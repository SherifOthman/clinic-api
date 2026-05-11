using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IUserRepository    _users;
    private readonly IEmailTokenService _emailTokenService;
    private readonly IAuditWriter       _audit;
    private readonly ILogger<ForgotPasswordHandler> _logger;

    public ForgotPasswordHandler(
        IUserRepository users,
        IEmailTokenService emailTokenService,
        IAuditWriter audit,
        ILogger<ForgotPasswordHandler> logger)
    {
        _users             = users;
        _emailTokenService = emailTokenService;
        _audit             = audit;
        _logger            = logger;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        var user = await _users.GetByEmailOrUsernameAsync(request.Email, ct);

        // Always return success — prevents email enumeration
        if (user is null)
        {
            _logger.LogInformation("Password reset OTP requested for non-existent email: {Email}", request.Email);
            return Result.Success();
        }

        try
        {
            await _emailTokenService.SendPasswordResetOtpAsync(user, ct);
            _logger.LogInformation("Password reset OTP sent to: {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset OTP to: {Email}", user.Email);
        }

        await _audit.WriteEventAsync("PasswordResetRequested",
            overrideUserId: user.Id, overrideFullName: user.FullName,
            overrideEmail: user.Email, ct: ct);

        return Result.Success();
    }
}
