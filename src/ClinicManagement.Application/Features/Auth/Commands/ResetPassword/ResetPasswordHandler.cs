using ClinicManagement.Application.Common.Extensions;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, ResetPasswordResult>
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<ResetPasswordHandler> _logger;

    public ResetPasswordHandler(
        UserManager<User> userManager,
        ILogger<ResetPasswordHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<ResetPasswordResult> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Password reset attempted for non-existent user: {Email}", request.Email);
            return new ResetPasswordResult(
                Success: false,
                ErrorCode: ErrorCodes.TOKEN_INVALID,
                ErrorMessage: "Invalid reset token"
            );
        }

        try
        {
            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            result.ThrowIfFailed();

            _logger.LogInformation("Password reset successfully for user: {UserId}", user.Id);
            
            return new ResetPasswordResult(
                Success: true,
                ErrorCode: null,
                ErrorMessage: null
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Password reset failed for user: {Email}", request.Email);
            return new ResetPasswordResult(
                Success: false,
                ErrorCode: ErrorCodes.TOKEN_INVALID,
                ErrorMessage: ex.Message
            );
        }
    }
}
