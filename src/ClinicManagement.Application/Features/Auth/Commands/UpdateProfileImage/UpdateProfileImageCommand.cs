using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.UpdateProfileImage;

public record UpdateProfileImageCommand : IRequest<Result<UserDto>>
{
    public string ProfileImageUrl { get; init; } = string.Empty;
}


public class UpdateProfileImageCommandHandler : IRequestHandler<UpdateProfileImageCommand, Result<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateProfileImageCommandHandler> _logger;

    public UpdateProfileImageCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<UpdateProfileImageCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<UserDto>> Handle(UpdateProfileImageCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.TryGetUserId(out var userId))
        {
            _logger.LogWarning("Unauthorized attempt to update profile image");
            return Result<UserDto>.FailSystem("UNAUTHENTICATED", "User is not authenticated");
        }

        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User not found for profile image update: {UserId}", userId);
            return Result<UserDto>.FailSystem("NOT_FOUND", "User not found");
        }

        try
        {
            user.ProfileImageUrl = string.IsNullOrWhiteSpace(request.ProfileImageUrl) ? null : request.ProfileImageUrl.Trim();

            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Profile image updated successfully for user {UserId}", userId);

            var userDto = user.Adapt<UserDto>();
            return Result<UserDto>.Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile image for user {UserId}", userId);
            return Result<UserDto>.FailSystem("INTERNAL_ERROR", "An error occurred while updating profile image");
        }
    }
}