using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Abstractions.Storage;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Exceptions;
using ClinicManagement.Domain.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Auth.Commands.ResendEmailVerification;

public record ResendEmailVerificationCommand(
    string Email
) : IRequest<Result>;

public class ResendEmailVerificationValidator : AbstractValidator<ResendEmailVerificationCommand>
{
    public ResendEmailVerificationValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format");
    }
}

public class ResendEmailVerificationHandler : IRequestHandler<ResendEmailVerificationCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailConfirmationService _emailConfirmationService;
    private readonly ILogger<ResendEmailVerificationHandler> _logger;

    public ResendEmailVerificationHandler(
        IUnitOfWork unitOfWork,
        IEmailConfirmationService emailConfirmationService,
        ILogger<ResendEmailVerificationHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _emailConfirmationService = emailConfirmationService;
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

        if (user.EmailConfirmed)
        {
            _logger.LogInformation("Resend email verification attempted for already confirmed email: {Email}", request.Email);
            return Result.Failure(ErrorCodes.EMAIL_ALREADY_CONFIRMED, "Email is already confirmed");
        }

        try
        {
            await _emailConfirmationService.SendConfirmationEmailAsync(user, cancellationToken);
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

