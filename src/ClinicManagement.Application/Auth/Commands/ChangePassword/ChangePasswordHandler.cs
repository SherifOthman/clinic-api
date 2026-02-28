using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Auth.Commands.ChangePassword;

public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<Domain.Entities.User> _userManager;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ChangePasswordHandler> _logger;

    public ChangePasswordHandler(
        IApplicationDbContext context,
        UserManager<Domain.Entities.User> userManager,
        ICurrentUserService currentUser,
        ILogger<ChangePasswordHandler> logger)
    {
        _context = context;
        _userManager = userManager;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetRequiredUserId();
        
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return Result.Failure(ErrorCodes.USER_NOT_FOUND, "User not found");
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Failed to change password for user {UserId}: {Errors}", user.Id, errors);
            return Result.Failure(ErrorCodes.INVALID_CREDENTIALS, "Current password is incorrect");
        }

        user.LastPasswordChangeAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Password changed successfully for user: {UserId}", user.Id);
        return Result.Success();
    }
}
