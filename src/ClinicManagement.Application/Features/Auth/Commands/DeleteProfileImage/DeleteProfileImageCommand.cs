using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Constants;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.DeleteProfileImage;

public record DeleteProfileImageCommand : IRequest<Result<UserDto>>;

public class DeleteProfileImageCommandHandler : IRequestHandler<DeleteProfileImageCommand, Result<UserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<DeleteProfileImageCommandHandler> _logger;

    public DeleteProfileImageCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService,
        ILogger<DeleteProfileImageCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<Result<UserDto>> Handle(DeleteProfileImageCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.TryGetUserId(out var userId))
        {
            return Result<UserDto>.Fail(MessageCodes.Authentication.USER_NOT_AUTHENTICATED);
        }

        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null)
        {
            return Result<UserDto>.Fail(MessageCodes.Authentication.USER_NOT_FOUND);
        }

        try
        {
            // Delete profile image if exists
            if (!string.IsNullOrWhiteSpace(user.ProfileImageUrl))
            {
                await _fileStorageService.DeleteFileAsync(user.ProfileImageUrl, cancellationToken);
                user.ProfileImageUrl = null;
                
                _context.Users.Update(user);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Profile image deleted successfully for user {UserId}", userId);
            }

            var userDto = user.Adapt<UserDto>();
            return Result<UserDto>.Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting profile image for user {UserId}", userId);
            return Result<UserDto>.Fail(MessageCodes.Exception.INTERNAL_SERVER_ERROR);
        }
    }
}