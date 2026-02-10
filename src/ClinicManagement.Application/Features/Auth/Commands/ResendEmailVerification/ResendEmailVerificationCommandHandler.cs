using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.ResendEmailVerification;

public record ResendEmailVerificationCommand : IRequest<Result>
{
    public string Email { get; init; } = string.Empty;
}

public class ResendEmailVerificationCommandHandler : IRequestHandler<ResendEmailVerificationCommand, Result>
{
    private readonly IUserManagementService _userManagementService;
    private readonly IEmailConfirmationService _emailConfirmationService;

    public ResendEmailVerificationCommandHandler(IUserManagementService userManagementService, IEmailConfirmationService emailConfirmationService)
    {
        _userManagementService = userManagementService;
        _emailConfirmationService = emailConfirmationService;
    }

    public async Task<Result> Handle(ResendEmailVerificationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManagementService.GetUserByEmailAsync(request.Email, cancellationToken);
        if (user == null)
            return Result.FailSystem("NOT_FOUND", "User not found");

        if (await _emailConfirmationService.IsEmailConfirmedAsync(user, cancellationToken))
            return Result.FailBusiness("EMAIL_ALREADY_CONFIRMED", "Email is already confirmed");

        var result = await _emailConfirmationService.SendConfirmationEmailAsync(user, cancellationToken);
        if (!result.Success)
            return Result.FailSystem("EMAIL_SEND_FAILED", "Failed to send confirmation email");

        return Result.Ok();
    }
}