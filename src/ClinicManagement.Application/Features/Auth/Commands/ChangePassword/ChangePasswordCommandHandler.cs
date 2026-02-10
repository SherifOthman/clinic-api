using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Interfaces;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.ChangePassword;

public record ChangePasswordCommand : IRequest<Result>
{
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserManagementService _userManagementService;

    public ChangePasswordCommandHandler(
        ICurrentUserService currentUserService,
        IUserManagementService userManagementService)
    {
        _currentUserService = currentUserService;
        _userManagementService = userManagementService;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
            return Result.FailSystem("UNAUTHENTICATED", "User is not authenticated");

        var user = await _userManagementService.GetUserByIdAsync(userId.Value, cancellationToken);
        if (user == null)
            return Result.FailSystem("NOT_FOUND", "User not found");

        if (!await _userManagementService.CheckPasswordAsync(user, request.CurrentPassword, cancellationToken))
            return Result.FailValidation("currentPassword", "Current password is incorrect");

        return Result.Ok();
    }
}