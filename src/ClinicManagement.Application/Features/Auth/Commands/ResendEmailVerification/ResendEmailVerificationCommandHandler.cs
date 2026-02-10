using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Constants;
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
            return Result.Fail(MessageCodes.Authentication.USER_WITH_EMAIL_NOT_FOUND);

        if (await _emailConfirmationService.IsEmailConfirmedAsync(user, cancellationToken))
            return Result.Fail(MessageCodes.Authentication.EMAIL_ALREADY_CONFIRMED);

        var result = await _emailConfirmationService.SendConfirmationEmailAsync(user, cancellationToken);
        if (!result.Success)
            return Result.Fail(result.Code ?? MessageCodes.Exception.INTERNAL_ERROR);

        return Result.Ok();
    }
}