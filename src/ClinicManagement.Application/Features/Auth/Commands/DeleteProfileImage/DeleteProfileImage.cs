using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Abstractions.Storage;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands;

public record DeleteProfileImageCommand() : IRequest<Result>;

public class DeleteProfileImageHandler : IRequestHandler<DeleteProfileImageCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<DeleteProfileImageHandler> _logger;

    public DeleteProfileImageHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IFileStorageService fileStorageService,
        ILogger<DeleteProfileImageHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<Result> Handle(
        DeleteProfileImageCommand request,
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

        if (!string.IsNullOrWhiteSpace(user.ProfileImageUrl))
        {
            await _fileStorageService.DeleteFileAsync(user.ProfileImageUrl, cancellationToken);
        }

        // Clear profile image URL
        user.ProfileImageUrl = null;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Profile image deleted successfully for user: {UserId}", user.Id);
        return Result.Success();
    }
}

