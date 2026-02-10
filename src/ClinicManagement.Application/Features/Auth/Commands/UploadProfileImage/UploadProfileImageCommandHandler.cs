using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.UploadProfileImage;

public record UploadProfileImageCommand : IRequest<Result<UserDto>>
{
    public IFormFile File { get; init; } = null!;
}

public class UploadProfileImageCommandHandler : IRequestHandler<UploadProfileImageCommand, Result<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<UploadProfileImageCommandHandler> _logger;

    public UploadProfileImageCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService,
        ILogger<UploadProfileImageCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<Result<UserDto>> Handle(UploadProfileImageCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.TryGetUserId(out var userId))
        {
            return Result<UserDto>.Fail(MessageCodes.Authentication.USER_NOT_AUTHENTICATED);
        }

        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return Result<UserDto>.Fail(MessageCodes.Authentication.USER_NOT_FOUND);
        }

        try
        {
            // Delete old profile image if exists
            if (!string.IsNullOrWhiteSpace(user.ProfileImageUrl))
            {
                await _fileStorageService.DeleteFileAsync(user.ProfileImageUrl, cancellationToken);
            }

            // Upload new profile image
            var filePath = await _fileStorageService.UploadFileAsync(request.File, "profiles", cancellationToken);
            
            user.ProfileImageUrl = filePath;
            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Profile image uploaded successfully for user {UserId}", userId);

            var userDto = user.Adapt<UserDto>();
            return Result<UserDto>.Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading profile image for user {UserId}", userId);
            return Result<UserDto>.Fail(MessageCodes.Exception.INTERNAL_SERVER_ERROR);
        }
    }
}