using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.UpdateProfile;

public class UpdateProfileHandler : IRequestHandler<UpdateProfileCommand, UpdateProfileResult>
{
    private readonly UserManager<User> _userManager;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UpdateProfileHandler> _logger;

    public UpdateProfileHandler(
        UserManager<User> userManager,
        ICurrentUserService currentUser,
        ILogger<UpdateProfileHandler> logger)
    {
        _userManager = userManager;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<UpdateProfileResult> Handle(
        UpdateProfileCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(_currentUser.UserId!.Value.ToString());
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", _currentUser.UserId);
            return new UpdateProfileResult(
                Success: false,
                ErrorCode: ErrorCodes.USER_NOT_FOUND,
                ErrorMessage: "User not found"
            );
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            _logger.LogWarning("Profile update failed for user {UserId}: {Errors}", user.Id, errors);
            return new UpdateProfileResult(
                Success: false,
                ErrorCode: ErrorCodes.OPERATION_FAILED,
                ErrorMessage: $"Failed to update profile: {errors}"
            );
        }

        _logger.LogInformation("Profile updated successfully for user: {UserId}", user.Id);
        return new UpdateProfileResult(
            Success: true,
            ErrorCode: null,
            ErrorMessage: null
        );
    }
}
