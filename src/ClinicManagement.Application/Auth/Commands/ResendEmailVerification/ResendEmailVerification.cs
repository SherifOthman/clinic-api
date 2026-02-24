using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Exceptions;
using ClinicManagement.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Auth.Commands.ResendEmailVerification;

public record ResendEmailVerificationCommand(
    string Email
) : IRequest<Result>;

public class ResendEmailVerificationHandler : IRequestHandler<ResendEmailVerificationCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailTokenService _emailTokenService;
    private readonly ILogger<ResendEmailVerificationHandler> _logger;

    public ResendEmailVerificationHandler(
        IUnitOfWork unitOfWork,
        IEmailTokenService emailTokenService,
        ILogger<ResendEmailVerificationHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _emailTokenService = emailTokenService;
        _logger = logger;
    }

    public async Task<Result> Handle(
        ResendEmailVerificationCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("Resend email verification attempted for non-existent user: {Email}", request.Email);
            return Result.Failure(ErrorCodes.USER_NOT_FOUND, "User not found");
        }

        if (user.IsEmailConfirmed)
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
