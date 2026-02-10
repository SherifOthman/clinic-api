using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Constants;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.UpdateProfile;

public record UpdateProfileCommand : IRequest<Result<UserDto>>
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string? ProfileImageUrl { get; init; }
}


public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result<UserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateProfileCommandHandler> _logger;

    public UpdateProfileCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<UpdateProfileCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<UserDto>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.TryGetUserId(out var userId))
        {
            _logger.LogWarning("Unauthorized attempt to update profile");
            return Result<UserDto>.Fail(MessageCodes.Authentication.USER_NOT_AUTHENTICATED);
        }

        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User not found for profile update: {UserId}", userId);
            return Result<UserDto>.Fail(MessageCodes.Authentication.USER_NOT_FOUND);
        }

        try
        {
            // Update user properties directly
            user.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();
            user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
            user.ProfileImageUrl = string.IsNullOrWhiteSpace(request.ProfileImageUrl) ? null : request.ProfileImageUrl.Trim();

            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Profile updated successfully for user {UserId}", userId);

            var userDto = user.Adapt<UserDto>();
            return Result<UserDto>.Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user {UserId}", userId);
            return Result<UserDto>.Fail(MessageCodes.Exception.INTERNAL_ERROR);
        }
    }
}