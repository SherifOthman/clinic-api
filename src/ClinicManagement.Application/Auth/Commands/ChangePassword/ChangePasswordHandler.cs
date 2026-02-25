using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Auth.Commands.ChangePassword;

public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ChangePasswordHandler> _logger;

    public ChangePasswordHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ICurrentUserService currentUser,
        ILogger<ChangePasswordHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetRequiredUserId();
        
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
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
        
        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("Password changed successfully for user: {UserId}", user.Id);
        return Result.Success();
    }
}
