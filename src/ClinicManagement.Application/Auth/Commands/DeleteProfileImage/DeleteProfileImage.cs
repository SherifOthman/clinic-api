using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Abstractions.Storage;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Auth.Commands;

public record DeleteProfileImageCommand() : IRequest<Result>;

public class DeleteProfileImageHandler : IRequestHandler<DeleteProfileImageCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<DeleteProfileImageHandler> _logger;

    public DeleteProfileImageHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IFileStorageService fileStorageService,
        ILogger<DeleteProfileImageHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<Result> Handle(
        DeleteProfileImageCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetRequiredUserId();
        
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return Result.Failure(ErrorCodes.USER_NOT_FOUND, "User not found");
        }


        if (!string.IsNullOrWhiteSpace(user.ProfileImageUrl))
        {
            await _fileStorageService.DeleteFileAsync(user.ProfileImageUrl, cancellationToken);
        }

        // Clear profile image URL
        user.ProfileImageUrl = null;

        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("Profile image deleted successfully for user: {UserId}", user.Id);
        return Result.Success();
    }
}

