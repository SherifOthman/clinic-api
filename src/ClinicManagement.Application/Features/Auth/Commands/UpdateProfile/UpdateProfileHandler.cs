using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.UpdateProfile;

public class UpdateProfileHandler : IRequestHandler<UpdateProfileCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UpdateProfileHandler> _logger;

    public UpdateProfileHandler(IUnitOfWork uow, ICurrentUserService currentUser, ILogger<UpdateProfileHandler> logger)
    {
        _uow = uow;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetRequiredUserId();
        var user = await _uow.Users.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return Result.Failure(ErrorCodes.USER_NOT_FOUND, "User not found");
        }

        user.FullName    = request.FullName.Trim();
        user.Gender      = Enum.TryParse<Domain.Enums.Gender>(request.Gender, out var g) ? g : Domain.Enums.Gender.Male;
        user.UserName    = request.userName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();

        await _uow.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Profile updated successfully for user: {UserId}", userId);
        return Result.Success();
    }
}
