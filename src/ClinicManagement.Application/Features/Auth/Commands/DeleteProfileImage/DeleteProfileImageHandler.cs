using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.DeleteProfileImage;

public class DeleteProfileImageHandler : IRequestHandler<DeleteProfileImageCommand, DeleteProfileImageResult>
{
    private readonly UserManager<User> _userManager;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<DeleteProfileImageHandler> _logger;

    public DeleteProfileImageHandler(
        UserManager<User> userManager,
        ICurrentUserService currentUser,
        IFileStorageService fileStorageService,
        ILogger<DeleteProfileImageHandler> logger)
    {
        _userManager = userManager;
        _currentUser = currentUser;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<DeleteProfileImageResult> Handle(
        DeleteProfileImageCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(_currentUser.UserId!.Value.ToString());
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", _currentUser.UserId);
            return new DeleteProfileImageResult(
                Success: false,
                ErrorCode: ErrorCodes.USER_NOT_FOUND,
                ErrorMessage: "User not found"
            );
        }

        // Delete profile image file if exists
        if (!string.IsNullOrWhiteSpace(user.ProfileImageUrl))
        {
            await _fileStorageService.DeleteFileAsync(user.ProfileImageUrl, cancellationToken);
        }

        // Clear profile image URL
        user.ProfileImageUrl = null;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            _logger.LogWarning("Profile image deletion failed for user {UserId}: {Errors}", user.Id, errors);
            return new DeleteProfileImageResult(
                Success: false,
                ErrorCode: ErrorCodes.OPERATION_FAILED,
                ErrorMessage: $"Failed to delete profile image: {errors}"
            );
        }

        _logger.LogInformation("Profile image deleted successfully for user: {UserId}", user.Id);
        return new DeleteProfileImageResult(
            Success: true,
            ErrorCode: null,
            ErrorMessage: null
        );
    }
}
