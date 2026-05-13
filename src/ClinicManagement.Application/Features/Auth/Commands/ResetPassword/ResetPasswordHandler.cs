using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IUserRepository   _users;
    private readonly IUnitOfWork       _uow;
    private readonly UserManager<User> _userManager;
    private readonly IEmailTokenService _emailTokenService;
    private readonly IAuditWriter      _audit;
    private readonly ILogger<ResetPasswordHandler> _logger;

    public ResetPasswordHandler(
        IUserRepository users,
        IUnitOfWork uow,
        UserManager<User> userManager,
        IEmailTokenService emailTokenService,
        IAuditWriter audit,
        ILogger<ResetPasswordHandler> logger)
    {
        _users              = users;
        _uow                = uow;
        _userManager        = userManager;
        _emailTokenService  = emailTokenService;
        _audit              = audit;
        _logger             = logger;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        var user = await _users.GetByEmailOrUsernameAsync(request.Email, ct);

        if (user is null)
        {
            _logger.LogWarning("Password reset attempted for non-existent user: {Email}", request.Email);
            return Result.Failure(ErrorCodes.TOKEN_INVALID, "Invalid or expired code");
        }

        if (!await _emailTokenService.VerifyPasswordResetOtpAsync(user, request.Otp, ct))
        {
            _logger.LogWarning("Invalid reset OTP for {Email}", request.Email);
            return Result.Failure(ErrorCodes.TOKEN_INVALID, "Invalid or expired code");
        }

        var removeResult = await _userManager.RemovePasswordAsync(user);
        if (!removeResult.Succeeded)
        {
            var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
            _logger.LogWarning("Failed to remove old password for {Email}: {Errors}", request.Email, errors);
            return Result.Failure(ErrorCodes.INTERNAL_ERROR, "Unable to reset password");
        }

        var addResult = await _userManager.AddPasswordAsync(user, request.NewPassword);
        if (!addResult.Succeeded)
        {
            var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
            _logger.LogWarning("Failed to set new password for {Email}: {Errors}", request.Email, errors);
            return Result.Failure(ErrorCodes.INTERNAL_ERROR, "Unable to reset password");
        }

        user.LastPasswordChangeAt = DateTimeOffset.UtcNow;
        await _uow.SaveChangesAsync(ct);

        await _audit.WriteEventAsync("PasswordReset",
            overrideUserId: user.Id, overrideFullName: user.FullName,
            overrideEmail: user.Email, ct: ct);

        _logger.LogInformation("Password reset successfully for user: {UserId}", user.Id);
        return Result.Success();
    }
}