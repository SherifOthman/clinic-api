using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.ConfirmEmail;

public class ConfirmEmailHandler : IRequestHandler<ConfirmEmailCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly IEmailTokenService _emailTokenService;
    private readonly ILogger<ConfirmEmailHandler> _logger;

    public ConfirmEmailHandler(IUnitOfWork uow, IEmailTokenService emailTokenService, ILogger<ConfirmEmailHandler> logger)
    {
        _uow               = uow;
        _emailTokenService = emailTokenService;
        _logger            = logger;
    }

    public async Task<Result> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        // Look up by ID — the token is cryptographically bound to this user ID.
        // Using email would require an extra lookup and would break if the user
        // changed their email between registration and clicking the link.
        var user = await _uow.Users.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("Email confirmation attempted for non-existent user: {UserId}", request.UserId);
            return Result.Failure(ErrorCodes.USER_NOT_FOUND, "User not found");
        }

        if (await _emailTokenService.IsEmailConfirmedAsync(user, cancellationToken))
        {
            _logger.LogInformation("Email already confirmed for user: {UserId}", user.Id);
            return Result.Failure(ErrorCodes.EMAIL_ALREADY_CONFIRMED, "Email is already confirmed");
        }

        try
        {
            await _emailTokenService.ConfirmEmailAsync(user, request.Token, cancellationToken);
            _logger.LogInformation("Email confirmed successfully for user: {UserId}", user.Id);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Email confirmation failed for user {UserId}: {Message}", user.Id, ex.Message);
            return Result.Failure(ErrorCodes.TOKEN_INVALID, ex.Message);
        }
    }
}
