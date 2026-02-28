using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Auth.Commands.UpdateProfile;

public class UpdateProfileHandler : IRequestHandler<UpdateProfileCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UpdateProfileHandler> _logger;

    public UpdateProfileHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        ILogger<UpdateProfileHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result> Handle(
        UpdateProfileCommand request,
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

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Profile updated successfully for user: {UserId}", user.Id);

        return Result.Success();
    }
}
