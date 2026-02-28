using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Auth.Commands.ResetPassword;

public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailTokenService _emailTokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<ResetPasswordHandler> _logger;

    public ResetPasswordHandler(
        IApplicationDbContext context,
        IEmailTokenService emailTokenService,
        IPasswordHasher passwordHasher,
        ILogger<ResetPasswordHandler> logger)
    {
        _context = context;
        _emailTokenService = emailTokenService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<Result> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
            
        if (user == null)
        {
            _logger.LogWarning("Password reset attempted for non-existent user: {Email}", request.Email);
            return Result.Failure(ErrorCodes.TOKEN_INVALID, "Invalid reset token");
        }

        if (!_emailTokenService.ValidatePasswordResetToken(user.Id, user.Email!, user.PasswordHash!, request.Token))
        {
            _logger.LogWarning("Invalid password reset token for user: {Email}", request.Email);
            return Result.Failure(ErrorCodes.TOKEN_INVALID, "Invalid or expired reset token");
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.LastPasswordChangeAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Password reset successfully for user: {UserId}", user.Id);

        return Result.Success();
    }
}
