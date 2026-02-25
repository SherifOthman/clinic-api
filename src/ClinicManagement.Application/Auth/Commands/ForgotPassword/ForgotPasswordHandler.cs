using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Common.Options;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Application.Auth.Commands.ForgotPassword;

public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailTokenService _emailTokenService;
    private readonly IEmailService _emailService;
    private readonly SmtpOptions _smtpOptions;
    private readonly ILogger<ForgotPasswordHandler> _logger;

    public ForgotPasswordHandler(
        IUnitOfWork unitOfWork,
        IEmailTokenService emailTokenService,
        IEmailService emailService,
        IOptions<SmtpOptions> smtpOptions,
        ILogger<ForgotPasswordHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _emailTokenService = emailTokenService;
        _emailService = emailService;
        _smtpOptions = smtpOptions.Value;
        _logger = logger;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);

        if (user == null)
        {
            _logger.LogInformation("Password reset requested for non-existent email: {Email}", request.Email);
            return Result.Success();
        }

        var token = _emailTokenService.GeneratePasswordResetToken(user.Id, user.Email!, user.PasswordHash);

        var displayName = $"{user.FirstName} {user.LastName}".Trim();
        var resetLink = $"{_smtpOptions.FrontendUrl}/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";

        try
        {
            await _emailService.SendPasswordResetEmailAsync(user.Email!, displayName, resetLink, cancellationToken);
            _logger.LogInformation("Password reset email sent to: {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to: {Email}", user.Email);
        }

        return Result.Success();
    }
}
