using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, ChangePasswordResult>
{
    private readonly UserManager<User> _userManager;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ChangePasswordHandler> _logger;

    public ChangePasswordHandler(
        UserManager<User> userManager,
        ICurrentUserService currentUser,
        ILogger<ChangePasswordHandler> logger)
    {
        _userManager = userManager;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ChangePasswordResult> Handle(
        ChangePasswordCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(_currentUser.UserId!.Value.ToString());
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", _currentUser.UserId);
            return new ChangePasswordResult(
                Success: false,
                ErrorCode: ErrorCodes.USER_NOT_FOUND,
                ErrorMessage: "User not found"
            );
        }

        if (!await _userManager.CheckPasswordAsync(user, request.CurrentPassword))
        {
            _logger.LogWarning("Invalid current password for user: {UserId}", user.Id);
            return new ChangePasswordResult(
                Success: false,
                ErrorCode: ErrorCodes.INVALID_CREDENTIALS,
                ErrorMessage: "Current password is incorrect"
            );
        }

        var result = await _userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Password change failed for user {UserId}: {Errors}", user.Id, errors);
            return new ChangePasswordResult(
                Success: false,
                ErrorCode: ErrorCodes.OPERATION_FAILED,
                ErrorMessage: errors
            );
        }

        _logger.LogInformation("Password changed successfully for user: {UserId}", user.Id);
        return new ChangePasswordResult(
            Success: true,
            ErrorCode: null,
            ErrorMessage: null
        );
    }
}
