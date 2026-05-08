using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Common.Options;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IUserRepository     _users;
    private readonly UserManager<User>   _userManager;
    private readonly IEmailService       _emailService;
    private readonly AppOptions          _appOptions;
    private readonly IAuditWriter        _audit;
    private readonly ILogger<ForgotPasswordHandler> _logger;

    public ForgotPasswordHandler(
        IUserRepository users,
        UserManager<User> userManager,
        IEmailService emailService,
        IOptions<AppOptions> appOptions,
        IAuditWriter audit,
        ILogger<ForgotPasswordHandler> logger)
    {
        _users        = users;
        _userManager  = userManager;
        _emailService = emailService;
        _appOptions   = appOptions.Value;
        _audit        = audit;
        _logger       = logger;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByEmailOrUsernameAsync(request.Email, cancellationToken);

        if (user is null)
        {
            _logger.LogInformation("Password reset requested for non-existent email: {Email}", request.Email);
            return Result.Success();
        }

        var token       = await _userManager.GeneratePasswordResetTokenAsync(user);
        var displayName = user.FullName;
        var resetLink   = $"{_appOptions.DashboardUrl}/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";

        try
        {
            await _emailService.SendPasswordResetEmailAsync(user.Email!, displayName, resetLink, cancellationToken);
            _logger.LogInformation("Password reset email sent to: {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to: {Email}", user.Email);
        }

        await _audit.WriteEventAsync("PasswordResetRequested",
            overrideUserId: user.Id, overrideFullName: user.FullName,
            overrideEmail: user.Email, ct: cancellationToken);

        return Result.Success();
    }
}
