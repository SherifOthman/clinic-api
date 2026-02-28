using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Auth.Commands.ChangePassword;

public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ChangePasswordHandler> _logger;

    public ChangePasswordHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ICurrentUserService currentUser,
        ILogger<ChangePasswordHandler> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetRequiredUserId();
        
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return Result.Failure(ErrorCodes.USER_NOT_FOUND, "User not found");
        }

        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            _logger.LogWarning("Invalid current password for user: {UserId}", user.Id);
            return Result.Failure(ErrorCodes.INVALID_CREDENTIALS, "Current password is incorrect");
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.LastPasswordChangeAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Password changed successfully for user: {UserId}", user.Id);
        return Result.Success();
    }
}
