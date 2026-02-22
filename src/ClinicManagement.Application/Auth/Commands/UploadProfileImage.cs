using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Abstractions.Storage;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Auth.Commands;

public record UploadProfileImageCommand(
    IFormFile File
) : IRequest<Result>;

public class UploadProfileImageHandler : IRequestHandler<UploadProfileImageCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<UploadProfileImageHandler> _logger;

    public UploadProfileImageHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IFileStorageService fileStorageService,
        ILogger<UploadProfileImageHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<Result> Handle(
        UploadProfileImageCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(_currentUser.UserId!.Value, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", _currentUser.UserId);
            return Result.Failure(ErrorCodes.USER_NOT_FOUND, "User not found");
        }

        try
        {

            if (!string.IsNullOrWhiteSpace(user.ProfileImageUrl))
            {
                await _fileStorageService.DeleteFileAsync(user.ProfileImageUrl, cancellationToken);
            }

            // Upload and validate using file storage service
            var filePath = await _fileStorageService.UploadFileWithValidationAsync(
                request.File,
                "ProfileImage",
                cancellationToken);


            user.ProfileImageUrl = filePath;

            await _unitOfWork.Users.UpdateAsync(user, cancellationToken);

            _logger.LogInformation("Profile image uploaded successfully for user: {UserId}", user.Id);
            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid file for profile image upload: {UserId}", user.Id);
            return Result.Failure(ErrorCodes.INVALID_FORMAT, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload profile image for user: {UserId}", user.Id);
            return Result.Failure(ErrorCodes.OPERATION_FAILED, "Failed to upload profile image");
        }
    }
}

