using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.ResendEmailVerification;

public class ResendEmailVerificationHandler : IRequestHandler<ResendEmailVerificationCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly IEmailTokenService _emailTokenService;
    private readonly ILogger<ResendEmailVerificationHandler> _logger;

    public ResendEmailVerificationHandler(IUnitOfWork uow, IEmailTokenService emailTokenService, ILogger<ResendEmailVerificationHandler> logger)
    {
        _uow               = uow;
        _emailTokenService = emailTokenService;
        _logger            = logger;
    }

    public async Task<Result> Handle(ResendEmailVerificationCommand request, CancellationToken cancellationToken)
    {
        var user = await _uow.Users.GetByEmailOrUsernameAsync(request.Email, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("Resend email verification attempted for non-existent user: {Email}", request.Email);
            return Result.Failure(ErrorCodes.USER_NOT_FOUND, "User not found");
        }

        if (user.EmailConfirmed)
        {
            _logger.LogInformation("Resend email verification attempted for already confirmed email: {Email}", request.Email);
            return Result.Failure(ErrorCodes.EMAIL_ALREADY_CONFIRMED, "Email is already confirmed");
        }

        try
        {
            await _emailTokenService.SendConfirmationEmailAsync(user, cancellationToken);
            _logger.LogInformation("Verification email resent to: {Email}", request.Email);
            return Result.Success();
        }
        catch (DomainException ex)
        {
            _logger.LogError(ex, "Failed to resend verification email to: {Email}", request.Email);
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}
