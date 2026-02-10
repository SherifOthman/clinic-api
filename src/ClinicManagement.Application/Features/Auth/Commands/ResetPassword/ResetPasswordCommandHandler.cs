using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.ResetPassword;

public record ResetPasswordCommand : IRequest<Result>
{
    public string Email { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IUserManagementService _userManagementService;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        IUserManagementService userManagementService,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _userManagementService = userManagementService;
        _logger = logger;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManagementService.GetUserByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("Password reset attempted for non-existent email: {Email}", request.Email);
            return Result.FailValidation("email", "Invalid reset token");
        }

        var result = await _userManagementService.ResetPasswordAsync(user, request.Token, request.NewPassword, cancellationToken);
        
        if (result.Success)
        {
            _logger.LogInformation("Password reset successful for user: {Email}", user.Email);
        }
        else
        {
            _logger.LogWarning("Password reset failed for user: {Email}", user.Email);
        }

        return result;
    }
}