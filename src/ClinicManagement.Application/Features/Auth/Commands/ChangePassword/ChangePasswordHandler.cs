using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IUserRepository     _users;
    private readonly IUnitOfWork         _uow;
    private readonly UserManager<User>   _userManager;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditWriter        _audit;
    private readonly ILogger<ChangePasswordHandler> _logger;

    public ChangePasswordHandler(
        IUserRepository users,
        IUnitOfWork uow,
        UserManager<User> userManager,
        ICurrentUserService currentUser,
        IAuditWriter audit,
        ILogger<ChangePasswordHandler> logger)
    {
        _users       = users;
        _uow         = uow;
        _userManager = userManager;
        _currentUser = currentUser;
        _audit       = audit;
        _logger      = logger;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetRequiredUserId();
        var user   = await _users.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return Result.Failure(ErrorCodes.USER_NOT_FOUND, "User not found");
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Failed to change password for user {UserId}", user.Id);
            await _audit.WriteEventAsync("PasswordChangeFailed", "Incorrect current password",
                overrideUserId: user.Id, overrideEmail: user.Email, ct: cancellationToken);
            return Result.Failure(ErrorCodes.INVALID_CREDENTIALS, "Current password is incorrect");
        }

        user.LastPasswordChangeAt = DateTimeOffset.UtcNow;
        await _uow.SaveChangesAsync(cancellationToken);

        await _audit.WriteEventAsync("PasswordChanged", ct: cancellationToken);

        _logger.LogInformation("Password changed successfully for user: {UserId}", user.Id);
        return Result.Success();
    }
}
