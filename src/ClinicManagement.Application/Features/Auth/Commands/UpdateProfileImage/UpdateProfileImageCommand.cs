using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Constants;
using FluentValidation;
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
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateProfileImageCommandHandler> _logger;

    public UpdateProfileImageCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<UpdateProfileImageCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<UserDto>> Handle(UpdateProfileImageCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.TryGetUserId(out var userId))
        {
            _logger.LogWarning("Unauthorized attempt to update profile image");
            return Result<UserDto>.Fail(MessageCodes.Authentication.USER_NOT_AUTHENTICATED);
        }

        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User not found for profile image update: {UserId}", userId);
            return Result<UserDto>.Fail(MessageCodes.Authentication.USER_NOT_FOUND);
        }

        try
        {
            user.ProfileImageUrl = string.IsNullOrWhiteSpace(request.ProfileImageUrl) ? null : request.ProfileImageUrl.Trim();

            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Profile image updated successfully for user {UserId}", userId);

            var userDto = user.Adapt<UserDto>();
            return Result<UserDto>.Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile image for user {UserId}", userId);
            return Result<UserDto>.Fail(MessageCodes.Exception.INTERNAL_ERROR);
        }
    }
}