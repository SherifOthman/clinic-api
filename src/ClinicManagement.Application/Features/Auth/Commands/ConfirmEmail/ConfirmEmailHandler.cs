using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.ConfirmEmail;

public class ConfirmEmailHandler : IRequestHandler<ConfirmEmailCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailTokenService _emailTokenService;
    private readonly ILogger<ConfirmEmailHandler> _logger;

    public ConfirmEmailHandler(
        IApplicationDbContext context,
        IEmailTokenService emailTokenService,
        ILogger<ConfirmEmailHandler> logger)
    {
        _context = context;
        _emailTokenService = emailTokenService;
        _logger = logger;
    }

    public async Task<Result> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
            
        if (user == null)
        {
            _logger.LogWarning("Email confirmation attempted for non-existent user: {Email}", request.Email);
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
