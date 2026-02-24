using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Auth.Commands.ResetPassword;

public record ResetPasswordCommand(
    string Email,
    string Token,
    string NewPassword
) : IRequest<Result>;

public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailTokenService _emailTokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<ResetPasswordHandler> _logger;

    public ResetPasswordHandler(
        IUnitOfWork unitOfWork,
        IEmailTokenService emailTokenService,
        IPasswordHasher passwordHasher,
        ILogger<ResetPasswordHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _emailTokenService = emailTokenService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<Result> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("Password reset attempted for non-existent user: {Email}", request.Email);
            return Result.Failure(ErrorCodes.TOKEN_INVALID, "Invalid reset token");
        }

        // Validate token
        if (!_emailTokenService.ValidatePasswordResetToken(user.Id, user.Email!, user.PasswordHash, request.Token))
        {
            _logger.LogWarning("Invalid password reset token for user: {Email}", request.Email);
            return Result.Failure(ErrorCodes.TOKEN_INVALID, "Invalid or expired reset token");
        }

        // Hash new password
        var passwordHash = _passwordHasher.HashPassword(request.NewPassword);

        user.PasswordHash = passwordHash;

        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("Password reset successfully for user: {UserId}", user.Id);

        return Result.Success();
    }
}
