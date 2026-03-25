using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Abstractions.Storage;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Auth.Commands;

public record UploadProfileImageCommand(
    IFormFile File
) : IRequest<Result>;

public class UploadProfileImageHandler : IRequestHandler<UploadProfileImageCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<UploadProfileImageHandler> _logger;

    public UploadProfileImageHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IFileStorageService fileStorageService,
        ILogger<UploadProfileImageHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<Result> Handle(
        UploadProfileImageCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetRequiredUserId();
        
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return Result.Failure(ErrorCodes.USER_NOT_FOUND, "User not found");
        }

        try
        {
            if (!string.IsNullOrWhiteSpace(user.ProfileImageUrl))
            {
                await _fileStorageService.DeleteFileAsync(user.ProfileImageUrl, cancellationToken);
            }

            // Upload and validate using file storage service
            var uploadResult = await _fileStorageService.UploadFileWithValidationAsync(
                request.File,
                "ProfileImage",
                cancellationToken);

            if (uploadResult.IsFailure)
            {
                _logger.LogWarning("Invalid file for profile image upload: {UserId} - {ErrorCode}", user.Id, uploadResult.ErrorCode);
                return Result.Failure(uploadResult.ErrorCode ?? "UPLOAD_FAILED", uploadResult.ErrorMessage ?? "File upload failed");
            }

            user.ProfileImageUrl = uploadResult.Value;

            await _context.SaveChangesAsync(cancellationToken);

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

