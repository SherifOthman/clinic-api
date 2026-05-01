using ClinicManagement.Application.Abstractions.Data;
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
    private readonly IUnitOfWork _uow;
    private readonly UserManager<User> _userManager;
    private readonly IAuditWriter _audit;
    private readonly ILogger<ResetPasswordHandler> _logger;

    public ResetPasswordHandler(
        IUnitOfWork uow,
        UserManager<User> userManager,
        IAuditWriter audit,
        ILogger<ResetPasswordHandler> logger)
    {
        _uow         = uow;
        _userManager = userManager;
        _audit       = audit;
        _logger      = logger;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _uow.Users.GetByEmailOrUsernameAsync(request.Email, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("Password reset attempted for non-existent user: {Email}", request.Email);
            return Result.Failure(ErrorCodes.TOKEN_INVALID, "Invalid reset token");
        }

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Invalid password reset token for user: {Email} - {Errors}", request.Email, errors);
            await _audit.WriteEventAsync("PasswordResetFailed", "Invalid or expired token",
                overrideUserId: user.Id, overrideFullName: user.Person?.FullName,
                overrideEmail: user.Email, ct: cancellationToken);
            return Result.Failure(ErrorCodes.TOKEN_INVALID, "Invalid or expired reset token");
        }

        user.LastPasswordChangeAt = DateTimeOffset.UtcNow;
        await _uow.SaveChangesAsync(cancellationToken);

        await _audit.WriteEventAsync("PasswordReset",
            overrideUserId: user.Id, overrideFullName: user.Person?.FullName,
            overrideEmail: user.Email, ct: cancellationToken);

        _logger.LogInformation("Password reset successfully for user: {UserId}", user.Id);
        return Result.Success();
    }
}
