using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Abstractions.Storage;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands;

public class UploadProfileImageHandler : IRequestHandler<UploadProfileImageCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<UploadProfileImageHandler> _logger;

    public UploadProfileImageHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        IFileStorageService fileStorageService,
        ILogger<UploadProfileImageHandler> logger)
    {
        _uow                = uow;
        _currentUser        = currentUser;
        _fileStorageService = fileStorageService;
        _logger             = logger;
    }

    public async Task<Result> Handle(UploadProfileImageCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetRequiredUserId();
        var user   = await _uow.Users.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return Result.Failure(ErrorCodes.USER_NOT_FOUND, "User not found");
        }

        try
        {
            if (!string.IsNullOrWhiteSpace(user.ProfileImageUrl))
                await _fileStorageService.DeleteFileAsync(user.ProfileImageUrl, cancellationToken);

            var uploadResult = await _fileStorageService.UploadFileWithValidationAsync(
                request.File, "ProfileImage", cancellationToken);

            if (uploadResult.IsFailure)
            {
                _logger.LogWarning("Invalid file for profile image upload: {UserId}", user.Id);
                return Result.Failure(uploadResult.ErrorCode ?? ErrorCodes.UPLOAD_FAILED, uploadResult.ErrorMessage ?? "File upload failed");
            }

            user.ProfileImageUrl = uploadResult.Value;
            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Profile image uploaded successfully for user: {UserId}", user.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload profile image for user: {UserId}", user.Id);
            return Result.Failure(ErrorCodes.OPERATION_FAILED, "Failed to upload profile image");
        }
    }
}
