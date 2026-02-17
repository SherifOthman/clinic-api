using ClinicManagement.API.Common.Constants;
using ClinicManagement.API.Common.Options;
using ClinicManagement.API.Entities;
using ClinicManagement.API.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ClinicManagement.API.Features.Auth;

public class ProfileService
{
    private readonly UserManager<User> _userManager;
    private readonly LocalFileStorageService _fileStorageService;
    private readonly FileStorageOptions _fileStorageOptions;

    public ProfileService(
        UserManager<User> userManager,
        LocalFileStorageService fileStorageService,
        IOptions<FileStorageOptions> fileStorageOptions)
    {
        _userManager = userManager;
        _fileStorageService = fileStorageService;
        _fileStorageOptions = fileStorageOptions.Value;
    }

    public async Task<Result> UpdateProfileAsync(
        User user,
        string firstName,
        string lastName,
        string? phoneNumber,
        CancellationToken ct = default)
    {
        user.FirstName = firstName.Trim();
        user.LastName = lastName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            return Result.Failure(ErrorCodes.OPERATION_FAILED, $"Failed to update profile: {errors}");
        }

        return Result.Success();
    }

    public async Task<Result> UploadProfileImageAsync(
        User user,
        IFormFile file,
        CancellationToken ct = default)
    {
        // Validate file
        var validationResult = ValidateProfileImage(file);
        if (!validationResult.IsSuccess)
        {
            return validationResult;
        }

        try
        {
            // Delete old profile image if exists
            if (!string.IsNullOrWhiteSpace(user.ProfileImageUrl))
            {
                await _fileStorageService.DeleteFileAsync(user.ProfileImageUrl, ct);
            }

            // Upload new profile image
            var filePath = await _fileStorageService.UploadFileAsync(
                file,
                _fileStorageOptions.ProfileImage.Folder,
                ct);

            // Update user profile image URL
            user.ProfileImageUrl = filePath;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                // Rollback: delete uploaded file
                await _fileStorageService.DeleteFileAsync(filePath, ct);

                var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                return Result.Failure(ErrorCodes.OPERATION_FAILED, $"Failed to update profile: {errors}");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ErrorCodes.INTERNAL_ERROR, $"An error occurred while uploading profile image: {ex.Message}");
        }
    }

    public async Task<Result> DeleteProfileImageAsync(
        User user,
        CancellationToken ct = default)
    {
        // Delete profile image file if exists
        if (!string.IsNullOrWhiteSpace(user.ProfileImageUrl))
        {
            await _fileStorageService.DeleteFileAsync(user.ProfileImageUrl, ct);
        }

        // Clear profile image URL
        user.ProfileImageUrl = null;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            return Result.Failure(ErrorCodes.OPERATION_FAILED, $"Failed to delete profile image: {errors}");
        }

        return Result.Success();
    }

    private Result ValidateProfileImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Result.Failure(ErrorCodes.REQUIRED_FIELD, "File is required");
        }

        if (file.Length > _fileStorageOptions.ProfileImage.MaxFileSizeBytes)
        {
            var maxSizeMB = _fileStorageOptions.ProfileImage.MaxFileSizeBytes / (1024 * 1024);
            return Result.Failure(ErrorCodes.INVALID_FORMAT, $"File size must not exceed {maxSizeMB}MB");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_fileStorageOptions.ProfileImage.AllowedExtensions.Contains(extension))
        {
            var allowedTypes = string.Join(", ", _fileStorageOptions.ProfileImage.AllowedExtensions);
            return Result.Failure(ErrorCodes.INVALID_FORMAT, $"File must be one of the following types: {allowedTypes}");
        }

        return Result.Success();
    }

    public record Result(bool IsSuccess, string? ErrorCode = null, string? ErrorMessage = null)
    {
        public static Result Success() => new(true);
        public static Result Failure(string errorCode, string errorMessage) => new(false, errorCode, errorMessage);
    }
}
