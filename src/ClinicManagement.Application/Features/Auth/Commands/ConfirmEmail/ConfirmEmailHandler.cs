using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.ConfirmEmail;

public class ConfirmEmailHandler : IRequestHandler<ConfirmEmailCommand, ConfirmEmailResult>
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailConfirmationService _emailConfirmationService;
    private readonly ILogger<ConfirmEmailHandler> _logger;

    public ConfirmEmailHandler(
        UserManager<User> userManager,
        IEmailConfirmationService emailConfirmationService,
        ILogger<ConfirmEmailHandler> logger)
    {
        _userManager = userManager;
        _emailConfirmationService = emailConfirmationService;
        _logger = logger;
    }

    public async Task<ConfirmEmailResult> Handle(
        ConfirmEmailCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Email confirmation attempted for non-existent user: {Email}", request.Email);
            return new ConfirmEmailResult(
                Success: false,
                AlreadyConfirmed: false,
                ErrorCode: ErrorCodes.USER_NOT_FOUND,
                ErrorMessage: "User not found"
            );
        }

        // Check if already confirmed
        if (await _emailConfirmationService.IsEmailConfirmedAsync(user, cancellationToken))
        {
            _logger.LogInformation("Email already confirmed for user: {UserId}", user.Id);
            return new ConfirmEmailResult(
                Success: true,
                AlreadyConfirmed: true,
                ErrorCode: null,
                ErrorMessage: null
            );
        }

        try
        {
            await _emailConfirmationService.ConfirmEmailAsync(user, request.Token, cancellationToken);
            _logger.LogInformation("Email confirmed successfully for user: {UserId}", user.Id);
            
            return new ConfirmEmailResult(
                Success: true,
                AlreadyConfirmed: false,
                ErrorCode: null,
                ErrorMessage: null
            );
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("Email confirmation failed for user {UserId}: {ErrorCode}", user.Id, ex.ErrorCode);
            return new ConfirmEmailResult(
                Success: false,
                AlreadyConfirmed: false,
                ErrorCode: ex.ErrorCode,
                ErrorMessage: ex.Message
            );
        }
    }
}
