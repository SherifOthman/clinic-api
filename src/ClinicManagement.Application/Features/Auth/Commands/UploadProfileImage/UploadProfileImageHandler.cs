using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.UploadProfileImage;

public class UploadProfileImageHandler : IRequestHandler<UploadProfileImageCommand, UploadProfileImageResult>
{
    private readonly UserManager<User> _userManager;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<UploadProfileImageHandler> _logger;

    public UploadProfileImageHandler(
        UserManager<User> userManager,
        ICurrentUserService currentUser,
        IFileStorageService fileStorageService,
        ILogger<UploadProfileImageHandler> logger)
    {
        _userManager = userManager;
        _currentUser = currentUser;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<UploadProfileImageResult> Handle(
        UploadProfileImageCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(_currentUser.UserId!.Value.ToString());
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", _currentUser.UserId);
            return new UploadProfileImageResult(
                Success: false,
                ErrorCode: ErrorCodes.USER_NOT_FOUND,
                ErrorMessage: "User not found"
            );
        }

        try
        {
            // Delete old profile image if exists
            if (!string.IsNullOrWhiteSpace(user.ProfileImageUrl))
            {
                await _fileStorageService.DeleteFileAsync(user.ProfileImageUrl, cancellationToken);
            }

            // Upload and validate using file storage service
            var filePath = await _fileStorageService.UploadFileWithValidationAsync(
                request.File,
                "ProfileImage",
                cancellationToken);

            // Update user profile image URL
            user.ProfileImageUrl = filePath;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                // Rollback: delete uploaded file
                await _fileStorageService.DeleteFileAsync(filePath, cancellationToken);

                var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                _logger.LogWarning("Profile image upload failed for user {UserId}: {Errors}", user.Id, errors);
                return new UploadProfileImageResult(
                    Success: false,
                    ErrorCode: ErrorCodes.OPERATION_FAILED,
                    ErrorMessage: $"Failed to update profile: {errors}"
                );
            }

            _logger.LogInformation("Profile image uploaded successfully for user: {UserId}", user.Id);
            return new UploadProfileImageResult(
                Success: true,
                ErrorCode: null,
                ErrorMessage: null
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid file for profile image upload: {UserId}", user.Id);
            return new UploadProfileImageResult(
                Success: false,
                ErrorCode: ErrorCodes.INVALID_FORMAT,
                ErrorMessage: ex.Message
            );
        }
    }
}
