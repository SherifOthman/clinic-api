using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<ResetPasswordHandler> _logger;

    public ResetPasswordHandler(
        IApplicationDbContext context,
        UserManager<User> userManager,
        ILogger<ResetPasswordHandler> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
            
        if (user == null)
        {
            _logger.LogWarning("Password reset attempted for non-existent user: {Email}", request.Email);
            return Result.Failure(ErrorCodes.TOKEN_INVALID, "Invalid reset token");
        }

        // Use Identity's built-in password reset
        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Invalid password reset token for user: {Email} - {Errors}", request.Email, errors);
            return Result.Failure(ErrorCodes.TOKEN_INVALID, "Invalid or expired reset token");
        }

        user.LastPasswordChangeAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Password reset successfully for user: {UserId}", user.Id);

        return Result.Success();
    }
}
