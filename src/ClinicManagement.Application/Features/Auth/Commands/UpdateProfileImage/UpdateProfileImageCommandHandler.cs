using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.UpdateProfileImage;

public class UpdateProfileImageCommandHandler : IRequestHandler<UpdateProfileImageCommand, Result<UpdateProfileImageResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<UpdateProfileImageCommandHandler> _logger;

    public UpdateProfileImageCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService,
        IDateTimeProvider dateTimeProvider,
        ILogger<UpdateProfileImageCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<Result<UpdateProfileImageResponse>> Handle(UpdateProfileImageCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.TryGetUserId(out var userId))
        {
            _logger.LogWarning("Unauthorized attempt to update profile image");
            return Result<UpdateProfileImageResponse>.Fail("User not authenticated");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User not found for profile image update: {UserId}", userId);
            return Result<UpdateProfileImageResponse>.Fail(ApplicationErrors.Authentication.USER_NOT_FOUND);
        }

        try
        {
            // Delete old profile image if exists
            if (!string.IsNullOrEmpty(user.ProfileImageUrl))
            {
                var oldImagePath = user.ProfileImageUrl.Replace(_fileStorageService.GetFileUrl(""), "");
                await _fileStorageService.DeleteFileAsync(oldImagePath, cancellationToken);
                _logger.LogInformation("Deleted old profile image for user {UserId}: {OldImagePath}", userId, oldImagePath);
            }

            // Upload new image
            using var imageStream = request.Image.OpenReadStream();
            var uploadResult = await _fileStorageService.UploadFileAsync(
                imageStream,
                request.Image.FileName,
                request.Image.ContentType,
                "profile-images",
                cancellationToken);

            if (!uploadResult.Success)
            {
                _logger.LogError("Failed to upload profile image for user {UserId}: {Error}", userId, uploadResult.Code ?? "Unknown error");
                return Result<UpdateProfileImageResponse>.Fail(uploadResult.Code ?? "Failed to upload image");
            }

            // Update user profile
            var now = _dateTimeProvider.UtcNow;
            user.ProfileImageUrl = uploadResult.Value?.FileUrl;
            user.ProfileImageFileName = uploadResult.Value?.FileName;
            user.ProfileImageUpdatedAt = now;

            await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new UpdateProfileImageResponse
            {
                ImageUrl = uploadResult.Value?.FileUrl ?? "",
                FileName = uploadResult.Value?.FileName ?? "",
                UpdatedAt = now
            };

            _logger.LogInformation("Profile image updated successfully for user {UserId}: {ImageUrl}", userId, response.ImageUrl);
            return Result<UpdateProfileImageResponse>.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile image for user {UserId}", userId);
            return Result<UpdateProfileImageResponse>.Fail("Failed to update profile image");
        }
    }
}