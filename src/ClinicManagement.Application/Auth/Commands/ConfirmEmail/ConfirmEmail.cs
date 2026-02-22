using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Exceptions;
using ClinicManagement.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Auth.Commands.ConfirmEmail;

public record ConfirmEmailCommand(
    string Email,
    string Token
) : IRequest<Result>;


public class ConfirmEmailHandler : IRequestHandler<ConfirmEmailCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailConfirmationService _emailConfirmationService;
    private readonly ILogger<ConfirmEmailHandler> _logger;

    public ConfirmEmailHandler(
        IUnitOfWork unitOfWork,
        IEmailConfirmationService emailConfirmationService,
        ILogger<ConfirmEmailHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _emailConfirmationService = emailConfirmationService;
        _logger = logger;
    }

    public async Task<Result> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("Email confirmation attempted for non-existent user: {Email}", request.Email);
            return Result.Failure(ErrorCodes.USER_NOT_FOUND, "User not found");
        }

        if (await _emailConfirmationService.IsEmailConfirmedAsync(user, cancellationToken))
        {
            _logger.LogInformation("Email already confirmed for user: {UserId}", user.Id);
            return Result.Failure(ErrorCodes.EMAIL_ALREADY_CONFIRMED, "Email is arelady confirmed");
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            await _emailConfirmationService.ConfirmEmailAsync(user, request.Token, cancellationToken);
            _logger.LogInformation("Email confirmed successfully for user: {UserId}", user.Id);
            return Result.Success();
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("Email confirmation failed for user {UserId}: {ErrorCode}", user.Id, ex.ErrorCode);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Email confirmation failed for user {UserId}: {Message}", user.Id, ex.Message);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result.Failure(ErrorCodes.TOKEN_INVALID, ex.Message);
        }
    }
}

